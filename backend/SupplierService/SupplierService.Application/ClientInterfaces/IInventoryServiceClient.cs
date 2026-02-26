namespace SupplierService.Application.ClientInterfaces;

/// <summary>
/// Interface để gọi gRPC IncreaseStock sang InventoryService.
/// Khi PO hoàn thành, truyền StoreId và danh sách (ingredientId, qty) để cộng kho.
/// </summary>
public interface IInventoryServiceClient
{
    Task IncreaseStockAsync(Guid storeId, IEnumerable<IngredientStockEntry> items, CancellationToken cancellationToken = default);
}

public record IngredientStockEntry(Guid IngredientId, decimal Quantity, decimal UnitPrice);
