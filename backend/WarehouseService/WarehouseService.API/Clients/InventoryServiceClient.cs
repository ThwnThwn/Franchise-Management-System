using Grpc.Net.Client;
using InventoryService.Grpc.Protos.Ingredient;
using WarehouseService.Application.ClientInterfaces;

namespace WarehouseService.API.Clients;

public class InventoryServiceClient : IInventoryServiceClient
{
    private readonly IngredientGrpc.IngredientGrpcClient _client;

    public InventoryServiceClient(IConfiguration configuration)
    {
        var rawUrl = configuration["INVENTORY_SERVICE_URL"] ?? "http://localhost:5005";
        var url = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                  rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? rawUrl : $"http://{rawUrl}";

        var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
        {
            HttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }
        });

        _client = new IngredientGrpc.IngredientGrpcClient(channel);
    }

    public async Task<bool> IngredientExistsAsync(Guid ingredientId)
    {
        try
        {
            var response = await _client.GetIngredientByIdAsync(
                new GetIngredientByIdRequest { Id = ingredientId.ToString() });
            return response?.IngredientRes != null;
        }
        catch (global::Grpc.Core.RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.NotFound)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}