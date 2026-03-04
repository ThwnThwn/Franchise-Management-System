using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Helper;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.AppDbContext;
using EngagementService.Infrastucture.Persistences.Cache;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

namespace EngagementService.Infrastucture.Persistences.Repository
{
    public class LoyaltyAccountRepository : GenericRepository<LoyaltyAccount>, ILoyaltyAccountRepository
    {

        public LoyaltyAccountRepository(IEngagementDbContext dbContext) : base(dbContext)
        {

        }



        public async Task<LoyaltyAccount?> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.LoyaltyAccounts
                .Include(x => x.CurrentTier)
                //.Include(x => x.PointLogs)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task<LoyaltyReportDto> GetLoyaltyReportDataAsync(DateTime? from, DateTime? to, string? tierId, CancellationToken ct)
        {
            var accountQuery = _dbContext.LoyaltyAccounts
                .Include(x => x.CurrentTier)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(tierId) && Guid.TryParse(tierId, out var gTierId))
            {
                accountQuery = accountQuery.Where(x => x.CurrentTierId == gTierId);
            }

            // 2. Query cho PointLogs để tính toán Earned/Redeemed
            var logQuery = _dbContext.PointLogs
                .Include(p => p.User)
                .AsNoTracking();

            if (from.HasValue) logQuery = logQuery.Where(l => l.CreatedAt >= from.Value);
            if (to.HasValue) logQuery = logQuery.Where(l => l.CreatedAt <= to.Value);

            // Nếu có lọc theo Tier, chỉ tính điểm của những User thuộc Tier đó
            if (!string.IsNullOrEmpty(tierId) && Guid.TryParse(tierId, out var filterTierId))
            {
                logQuery = logQuery.Where(l => l.User!.CurrentTierId == filterTierId);
            }

            // 3. Thực thi truy vấn điểm
            var pointsSummary = await logQuery
                .GroupBy(l => 1) // Group giả để sum nhiều cột một lúc
                .Select(g => new
                {
                    Earned = g.Where(l => l.BalanceAfter > l.BalanceBefore).Sum(l => l.Amount),
                    Redeemed = g.Where(l => l.BalanceAfter < l.BalanceBefore).Sum(l => l.Amount)
                })
                .FirstOrDefaultAsync(ct);

            // 4. Lấy phân bổ Tier
            var tierDistribution = await accountQuery
                .GroupBy(x => new { x.CurrentTier.Name, x.CurrentTier.MinSpendRequired })
                .Select(g => new TierDistributionDto
                {
                    TierName = g.Key.Name,
                    MemberCount = g.Count(),
                    MinSpend = g.Key.MinSpendRequired // Giả định DTO của bạn có field này
                })
                .OrderByDescending(x => x.MinSpend)
                .ToListAsync(ct);

            return new LoyaltyReportDto
            {
                TotalMembers = await accountQuery.CountAsync(ct),
                TotalPointsEarned = pointsSummary?.Earned ?? 0,
                TotalPointsRedeemed = Math.Abs(pointsSummary?.Redeemed ?? 0),
                TierDistribution = tierDistribution
            };
        }

        public async Task<IEnumerable<LoyaltyAccount?>> GetUserByIdsAsync(List<Guid> userIds)
        {
            return await _dbContext.LoyaltyAccounts
                .Include(x => x.CurrentTier)
                .Include(x => x.PointLogs)
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToListAsync();
        }
        public async Task<IEnumerable<LoyaltyAccount?>> GetByFilterHandler(Guid? tierId, decimal? startAt, decimal? endAt)
        {
            var query = _dbContext.LoyaltyAccounts
            .Include(x => x.CurrentTier)
            .Include(x => x.PointLogs)
            .AsNoTracking()
            .AsQueryable();

            if (tierId.HasValue)
            {
                query = query.Where(x =>
                    x.CurrentTier != null &&
                    x.CurrentTier.Id == tierId.Value);
            }

            if (startAt.HasValue)
            {
                query = query.Where(x => x.TotalSpent >= startAt.Value);
            }

            if (endAt.HasValue)
            {
                query = query.Where(x => x.TotalSpent <= endAt.Value);
            }

            return await query.ToListAsync();
        }

        public async Task RedeemVoucherAsync(IPointRuleCache _pointRuleCache,Guid accountId, Guid voucherId, CancellationToken ct)
        {
            var account = _dbContext.LoyaltyAccounts
                .Include(a => a.UserVouchers)
                .Include(a => a.PointLogs)
                .FirstOrDefault(a => a.Id == accountId);
            
            var voucher = _dbContext.Vouchers.FirstOrDefault(v => v.Id == voucherId);

            VerifyAccountAndVoucher(account, voucher);

            var balanceBefore = account!.AvailablePoints;
            var balanceAfter = account.AvailablePoints - (voucher!.PointCost ?? 0);
            var pointToDeduct = voucher.PointCost ?? 0;

            account.AvailablePoints -= voucher.PointCost ?? 0;
            account.UserVouchers.Add(new UserVoucher
            {
                VoucherId = voucher.Id,
                UserId = account.Id,
                AcquiredAt = DateTimeOffset.UtcNow,
                Voucher = voucher,
            });

            //sinh point log
            var allRules = await _pointRuleCache.GetOrSetAllAsync(async () =>
            {
                // Gọi Repository để lấy dữ liệu (IEnumerable)
                var data = await _dbContext.PointRules.ToListAsync();

                // Chuyển đổi sang List để khớp với yêu cầu của hàm Cache
                return data.ToList();
            });
            var pointLog = HandlePointLog.CreatePointLog(account, balanceBefore, balanceAfter, pointToDeduct, voucherId, PointReasonCode.POINT_REDEEM, PointActionType.REDEEM, "Points deducted for voucher " + voucher.Code, allRules);

            account.PointLogs.Add(pointLog);

            if (voucher.CurrentQuantity.HasValue) voucher.CurrentQuantity -= 1;
        }

        private void VerifyAccountAndVoucher(LoyaltyAccount? account, Voucher? voucher)
        {
            if (account == null)
                throw new ArgumentException(LoyaltyAccountMsg.NotFound);

            if (voucher == null)
                throw new ArgumentException(VoucherMsg.NotFound);

            if (!voucher.IsActive)
                throw new InvalidOperationException(VoucherMsg.InactiveVoucher);

            if (voucher.EndDate.HasValue && voucher.EndDate.Value < DateTimeOffset.UtcNow)
                throw new InvalidOperationException(VoucherMsg.ExpiredVoucher);

            if (voucher.MaxUsagePerUser.HasValue)
            {
                var usageCount = account.UserVouchers.Count(uv => uv.VoucherId == voucher.Id);
                if (usageCount >= voucher.MaxUsagePerUser.Value)
                    throw new InvalidOperationException(VoucherMsg.UsageLimitReached);
            }

            if (voucher.PointCost != null && account.AvailablePoints < voucher.PointCost)
                throw new InvalidOperationException(LoyaltyAccountMsg.InsufficientPoints);

            if (voucher.CurrentQuantity <= 0)
                throw new InvalidOperationException(VoucherMsg.OutOfStock);
        }
    }
}
