using InventoryService.Application.Constants;
using Microsoft.Extensions.Configuration;
using Grpc.Net.Client;
using SupplierService.Grpc.Protos.Supplier;

namespace InventoryService.API.Services.Supplier
{
    public class SupplierServiceClient : Application.ClientInterface.ISupplierServiceClient
    {
        private readonly PurchaseOrderGrpc.PurchaseOrderGrpcClient _client;

        public SupplierServiceClient(IConfiguration configuration)
        {
            var rawUrl = configuration["GrpcSettings:SupplierServiceUrl"] ?? configuration["ServiceUrls:SupplierService"] ?? "https://localhost:5008";

            var url = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                      rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? rawUrl
                : $"https://{rawUrl}";

            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions { HttpHandler = httpHandler });
            _client = new PurchaseOrderGrpc.PurchaseOrderGrpcClient(channel);
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
