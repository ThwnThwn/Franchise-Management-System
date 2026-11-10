using InventoryService.Application.Constants;
using Microsoft.Extensions.Configuration;
using Grpc.Net.Client;
using SupplierService.Grpc.Protos.Supplier;

namespace InventoryService.API.Services.Supplier
{
    public class SupplierServiceClient : Application.ClientInterface.ISupplierServiceClient
    {
        private readonly PurchaseOrderGrpc.PurchaseOrderGrpcClient _client;

        public SupplierServiceClient(PurchaseOrderGrpc.PurchaseOrderGrpcClient client)
        {
           _client = client;
        }

        public async Task<Dictionary<Guid, decimal>> GetLatestPriceForIngredientsAsync(IEnumerable<Guid> ingredientIds, CancellationToken cancellationToken = default)
        {
            var request = new GetLatestPriceForIngredientsRequest();
            request.IngredientIds.AddRange(ingredientIds.Select(id => id.ToString()));

            var response = await _client.GetLatestPriceForIngredientsAsync(request, cancellationToken: cancellationToken);

            return response.Prices.ToDictionary(k => Guid.Parse(k.Key), v => (decimal)v.Value);
        }
    }
}
