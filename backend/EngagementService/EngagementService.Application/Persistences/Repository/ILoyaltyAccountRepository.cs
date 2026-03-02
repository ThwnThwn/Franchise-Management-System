using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Persistences.Repository
{
    public interface ILoyaltyAccountRepository : IGenericRepository<LoyaltyAccount>
    {
        Task<LoyaltyAccount?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<LoyaltyAccount?>> GetUserByIdsAsync(List<Guid> userIds);

        // Hàm lấy dữ liệu thô phục vụ báo cáo
        Task<LoyaltyReportDto> GetLoyaltyReportDataAsync(DateTime? from, DateTime? to, string? tierId, CancellationToken ct);
    }
}
