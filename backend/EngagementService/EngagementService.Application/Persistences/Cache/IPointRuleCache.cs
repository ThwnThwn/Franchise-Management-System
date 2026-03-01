using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Persistences.Cache
{
    public interface IPointRuleCache
    {
        Task SetPointRuleAsync(Guid ruleId, PointRule data);
        Task<PointRule?> GetPointRuleAsync(Guid ruleId);
        Task ClearAllPointRuleCacheAsync();
        Task<List<PointRule>> GetOrSetAllAsync(Func<Task<List<PointRule>>> getAllFromDb);
    }
}
