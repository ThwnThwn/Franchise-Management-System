using MediatR;

namespace InventoryService.Application.Features.StoreIngredient.Commands.CaculateCostIngredientInStock;

/// <summary>
/// Command tính MAC (Moving Average Cost) cho 1 nguyên liệu.
/// 
/// Có thể gọi trực tiếp qua gRPC (Postman test),
/// hoặc được gọi nội bộ từ IncreaseStockCommandHandler.
///
/// Công thức MAC:
///   NewCost = ((TồnCũ × CostCũ) + (SốLượngNhập × GiáNhập)) / (TồnCũ + SốLượngNhập)
/// </summary>
public class CaculateCostIngredientInStockCommand : IRequest<CaculateCostIngredientInStockResult>
{
    public Guid IngredientId { get; set; }
    public decimal AddedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CaculateCostIngredientInStockResult
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public decimal OldCost { get; set; }
    public decimal NewCost { get; set; }
    public decimal TotalStockBefore { get; set; }
    public decimal TotalStockAfter { get; set; }
    public string Message { get; set; } = string.Empty;
}
