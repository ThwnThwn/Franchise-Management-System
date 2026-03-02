using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.AppDbContext;
using Microsoft.EntityFrameworkCore;

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
    }
}
