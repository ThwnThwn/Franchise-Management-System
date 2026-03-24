using Grpc.Net.Client;
using InventoryService.Grpc.Protos.Ingredient;
using WarehouseService.Application.ClientInterfaces;

namespace WarehouseService.API.Clients;

public class InventoryServiceClient(IngredientGrpc.IngredientGrpcClient client) : IInventoryServiceClient
{
    public async Task<bool> IngredientExistsAsync(Guid ingredientId)
    {
        try
        {
            var response = await client.GetIngredientByIdAsync(
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