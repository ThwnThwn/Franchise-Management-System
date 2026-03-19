using System.Linq.Expressions;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace EngagementService.Infrastucture.Persistences.Repository
{
    public class CampaginRepository : GenericRepository<Campaign>, ICampaignRepository
    {
        public CampaginRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Campaign>> GetAllCampaign(
            Expression<Func<Campaign, bool>>? predicate = null
        )
        {
            var query = _dbContext.Campaigns.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query
                 .Include(c => c.Vouchers)
                    .ThenInclude(v => v.VoucherItems)
                 .ToListAsync();
        }

        public async Task<Campaign?> GetCampaignById(Guid id)
        {
            return await _dbContext.Campaigns
                 .Where(c => c.Id == id)
                 .Include(c => c.Vouchers)
                    .ThenInclude(v => v.VoucherItems)
                 .FirstOrDefaultAsync();
        }

        public async Task<Campaign?> GetCampaignByName(string name)
        {
            return await _dbContext.Campaigns
                 .Where(c => c.Name == name)
                 .Include(c => c.Vouchers)
                    .ThenInclude(v => v.VoucherItems)
                 .FirstOrDefaultAsync();
        }
    }
}