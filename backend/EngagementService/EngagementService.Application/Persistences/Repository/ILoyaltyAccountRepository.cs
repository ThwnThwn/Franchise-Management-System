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
    }
}
