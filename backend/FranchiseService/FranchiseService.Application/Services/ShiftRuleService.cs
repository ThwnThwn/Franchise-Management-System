using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Services;

/// <summary>
/// Interface cho dich vu truy van ShiftRule.
/// Gom logic: lay Rule theo Store, neu khong co thi fallback ve Rule Global.
/// </summary>
public interface IShiftRuleService
{
    /// <summary>
    /// Lay ShiftRule hieu luc cho mot Store.
    /// Uu tien Rule rieng theo StoreId, neu khong co thi lay Rule Global (StoreId = null).
    /// </summary>
    Task<ShiftRule?> GetEffectiveRuleAsync(Guid? storeId);
}

/// <summary>
/// Service truy van ShiftRule theo chien luoc Store fallback Global.
/// Duoc inject de tai su dung o nhieu Handler, tranh trung lap code.
/// </summary>
public class ShiftRuleService : IShiftRuleService
{
    private readonly IGenericRepository<ShiftRule> _ruleRepository;

    public ShiftRuleService(IGenericRepository<ShiftRule> ruleRepository)
    {
        _ruleRepository = ruleRepository;
    }

    /// <summary>
    /// Lay Rule hieu luc: tim theo StoreId truoc, khong co thi lay Global.
    /// </summary>
    public async Task<ShiftRule?> GetEffectiveRuleAsync(Guid? storeId)
    {
        // Tim Rule cau hinh rieng cho Store
        if (storeId.HasValue)
        {
            var storeRules = await _ruleRepository.GetAllAsync(
                predicate: r => r.StoreId == storeId.Value);
            var storeRule = storeRules.FirstOrDefault();
            if (storeRule != null) return storeRule;
        }

        // Fallback: lay Rule Global (StoreId = null)
        var globalRules = await _ruleRepository.GetAllAsync(
            predicate: r => r.StoreId == null);
        return globalRules.FirstOrDefault();
    }
}
