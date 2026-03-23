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
    public class MembershipTierRepository : GenericRepository<MembershipTier>, IMembershipTierRepository
    {
        public MembershipTierRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }
        /// <summary>
        /// Gets the default tier asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<MembershipTier?> GetDefaultTierAsync()
        {
            return await _dbContext.MembershipTiers
                .OrderBy(t => t.MinSpendRequired)
                .FirstOrDefaultAsync();
        }
    }
}
