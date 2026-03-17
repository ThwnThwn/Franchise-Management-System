using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Infrastucture.Persistences.Repository
{
    public class CampaginRepository : GenericRepository<Campaign>, ICampaignRepository
    {
        public CampaginRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
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
