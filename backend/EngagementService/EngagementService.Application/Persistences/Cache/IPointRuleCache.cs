using EngagementService.Domain.Entities;

namespace EngagementService.Application.Persistences.Cache;

public interface IPointRuleCache
{
    Task SetPointRuleAsync(Guid ruleId, PointRule data);

    Task<PointRule?> GetPointRuleAsync(Guid ruleId);

    Task ClearAllPointRuleCacheAsync();

    Task<List<PointRule>> GetOrSetAllAsync(Func<Task<List<PointRule>>> getAllFromDb);
}