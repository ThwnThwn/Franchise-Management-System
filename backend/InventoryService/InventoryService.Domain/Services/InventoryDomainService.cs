using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Services;

/// <summary>
/// Service chứa các logic nghiệp vụ cốt lõi của kho hàng.
/// </summary>
public class InventoryDomainService : IInventoryDomainService
{
    /// <summary>
    /// Tính toán tổng lượng nguyên liệu cần thiết dựa trên danh sách món và công thức.
    /// Logic: Duyệt qua từng món -> Duyệt qua công thức món đó -> Cộng dồn số lượng nguyên liệu cần.
    /// </summary>
    /// <param name="items">Danh sách các món (VariantId, Số lượng).</param>
    /// <param name="allRecipes">Danh sách công thức của các món đó.</param>
    /// <returns>Dictionary: Key = IngredientId, Value = Tổng số lượng cần.</returns>
    public Dictionary<Guid, decimal> CalculateRequiredIngredients(IEnumerable<(Guid VariantId, int Quantity)> items, IEnumerable<Recipe> allRecipes)
    {
        var totalRequired = new Dictionary<Guid, decimal>();

        // Gom nhóm công thức theo VariantId để tra cứu nhanh hơn
        var recipeLookup = allRecipes
            .GroupBy(r => r.VariantId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var item in items)
        {
            if (recipeLookup.TryGetValue(item.VariantId, out var recipes))
            {
                foreach (var recipe in recipes)
                {
                    var totalNeeded = recipe.Quantity * item.Quantity;

                    if (totalRequired.ContainsKey(recipe.IngredientId))
                    {
                        totalRequired[recipe.IngredientId] += totalNeeded;
                    }
                    else
                    {
                        totalRequired[recipe.IngredientId] = totalNeeded;
                    }
                }
            }
        }

        return totalRequired;
    }

    /// <summary>
    /// Kiểm tra xem nguyên liệu có đang ở mức thấp (Low Stock) hay không.
    /// </summary>
    /// <param name="currentQuantity">Số lượng tồn kho hiện tại.</param>
    /// <param name="minThreshold">Mức cảnh báo tối thiểu (Threshold).</param>
    /// <returns>True nếu tồn kho < mức tối thiểu.</returns>
    public bool IsLowStock(decimal currentQuantity, decimal minThreshold)
    {
        // Nếu minThreshold <= 0 thì coi như không cấu hình mức thấp -> Không báo động
        if (minThreshold <= 0) return false;

        return currentQuantity < minThreshold;
    }
}
