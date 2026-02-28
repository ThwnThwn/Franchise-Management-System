using Grpc.Net.Client;
using InventoryService.Grpc.Protos.Inventory;
using SupplierService.Application.ClientInterfaces;

namespace SupplierService.API.Clients.Inventory;

/// <summary>
/// Gọi gRPC IncreaseStock sang InventoryService để cộng kho khi PO hoàn thành.
/// </summary>
public class InventoryServiceClient : IInventoryServiceClient
{
    private readonly InventoryGrpc.InventoryGrpcClient _grpcClient;

    public InventoryServiceClient(IConfiguration configuration)
    {
        var rawUrl = configuration["INVENTORY_SERVICE_URL"] ?? "https://localhost:5005";
        var inventoryServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                  rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? rawUrl
            : $"https://{rawUrl}";

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var channel = GrpcChannel.ForAddress(inventoryServiceUrl, new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        _grpcClient = new InventoryGrpc.InventoryGrpcClient(channel);
    }

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

        await _grpcClient.IncreaseStockAsync(request, cancellationToken: cancellationToken);
    }
}
