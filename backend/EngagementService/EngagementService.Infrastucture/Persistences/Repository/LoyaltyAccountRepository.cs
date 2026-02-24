using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.AppDbContext;

namespace EngagementService.Infrastucture.Persistences.Repository
{
    public class LoyaltyAccountRepository : GenericRepository<LoyaltyAccount>, ILoyaltyAccountRepository
    {
        public LoyaltyAccountRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }
    }
}
