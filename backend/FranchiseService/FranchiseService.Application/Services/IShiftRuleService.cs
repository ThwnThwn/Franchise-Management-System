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
