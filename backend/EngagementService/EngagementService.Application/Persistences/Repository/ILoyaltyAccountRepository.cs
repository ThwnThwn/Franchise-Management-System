using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Domain.Entities;

namespace EngagementService.Application.Persistences.Repository
{
    public interface ILoyaltyAccountRepository : IGenericRepository<LoyaltyAccount>
    {
        Task<LoyaltyAccount?> GetByUserIdAsync(Guid userId);

        Task<IEnumerable<LoyaltyAccount?>> GetByFilterHandler(Guid? tierId, decimal? startAt, decimal? endAt);
        Task<IEnumerable<LoyaltyAccount?>> GetUserByIdsAsync(List<Guid> userIds);

        // Hàm lấy dữ liệu thô phục vụ báo cáo
        Task<LoyaltyReportDto> GetLoyaltyReportDataAsync(DateTime? from, DateTime? to, string? tierId, CancellationToken ct);
    
        Task RedeemVoucherAsync(IPointRuleCache _pointRuleCache, Guid accountId, Guid voucherId, CancellationToken ct);
    }
}
