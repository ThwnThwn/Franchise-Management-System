using MediatR;

namespace InventoryService.Application.Features.StoreIngredient.Commands.RecalculateVariantCosts;

/// <summary>
/// Command để tính lại cost cho tất cả Product Variant sử dụng 1 Ingredient.
/// Logic: Lấy Recipe → Tính SUM(ingredient.cost * recipe.quantity) → Update ProductVariant.Cost qua gRPC
/// </summary>
public class RecalculateVariantCostsByIngredientCommand : IRequest<RecalculateVariantCostsResult>
{
    public Guid IngredientId { get; set; }
}

public class RecalculateVariantCostsResult
{
    public List<VariantCostItem> Results { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class VariantCostItem
{
    public Guid VariantId { get; set; }
    public decimal OldCost { get; set; }
    public decimal NewCost { get; set; }
}
