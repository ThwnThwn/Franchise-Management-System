using Grpc.Net.Client;
using InventoryService.Grpc.Protos.Inventory;
using SupplierService.Application.ClientInterfaces;

namespace SupplierService.API.Clients.Inventory;

/// <summary>
/// Gọi gRPC IncreaseStock sang InventoryService để cộng kho khi PO hoàn thành.
/// </summary>
public class InventoryServiceClient(InventoryGrpc.InventoryGrpcClient grpcClient) : IInventoryServiceClient
{
    public async Task IncreaseStockAsync(Guid storeId, IEnumerable<IngredientStockEntry> items, CancellationToken cancellationToken = default)
    {
        var request = new IncreaseStockRequest
        {
            StoreId = storeId.ToString()
        };

        foreach (var item in items)
        {
            request.Items.Add(new IngredientStockItem
            {
                IngredientId = item.IngredientId.ToString(),
                Quantity = (double)item.Quantity,
                UnitPrice = (double)item.UnitPrice
            });
        }

        await grpcClient.IncreaseStockAsync(request, cancellationToken: cancellationToken);
    }
}
