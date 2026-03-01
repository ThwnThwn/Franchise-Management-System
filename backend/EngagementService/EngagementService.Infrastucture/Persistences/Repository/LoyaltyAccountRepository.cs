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
                .Include(x => x.PointLogs)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);
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
    }
}
