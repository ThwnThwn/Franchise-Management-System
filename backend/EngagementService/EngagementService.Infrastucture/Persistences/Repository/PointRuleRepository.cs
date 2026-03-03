using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
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
    public class PointRuleRepository : GenericRepository<PointRule>, IPointRuleRepository
    {
        public PointRuleRepository(IEngagementDbContext dbContext) : base(dbContext)
        {

        }

        //public async Task<PointRule?> GetByReasonCodeAsync(PointReasonCode actionType)
        //{
        //    var pointRule = await _dbContext.PointRules
        //        .Where(r => r.ReasonCode == actionType)
        //        .FirstOrDefaultAsync();
        //    return pointRule;
        //}
    }
}
