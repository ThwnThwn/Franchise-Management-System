using Grpc.Net.Client;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.Product;
using MediatR;
using ProductService.Grpc;

namespace InventoryService.API.Services.Product
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly string _productServiceClient;
        private readonly ProductVariantGrpc.ProductVariantGrpcClient _client;
        public ProductServiceClient(IMediator mediator, IConfiguration configuration)
        {
            _configuration = configuration;

            var rawUrl = configuration[Common.PRODUCT_SERVICE_URL] ?? "http://localhost:5002";
            // Allow env var values like "localhost:5002" by normalizing to an absolute URI.
            _productServiceClient = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                 rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? rawUrl
                : $"http://{rawUrl}";

            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress(_productServiceClient, new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

            _client = new ProductVariantGrpc.ProductVariantGrpcClient(channel);
        }
        public async Task<ProductVariantRespone> GetVariantById(Guid request)
        {
            var req = new GetProductVariantByIdRequest
            {
                ProductVariantId = request.ToString()
            };
            var respone = await _client.GetByIdAsync(req);
            return new ProductVariantRespone()
            {
                Id = Guid.Parse(respone.Id),
                Sku = respone.Sku,
                Name = respone.Name,
                Attributes = respone.Attributes,
                BasePrice = (decimal)respone.BasePrice,
                Cost = (decimal)respone.Cost,
                IsActive = respone.IsActive,
                ProductId = Guid.Parse(respone.ProductId)
            };  
        }

        public async Task UpdateVariantCostsAsync(Dictionary<Guid, decimal> variantCosts, CancellationToken cancellationToken = default)
        {
            var request = new UpdateVariantCostsRequest();
            foreach (var (variantId, newCost) in variantCosts)
            {
                request.Updates.Add(new VariantCostUpdate
                {
                    VariantId = variantId.ToString(),
                    NewCost = (double)newCost
                });
            }
            await _client.UpdateVariantCostsAsync(request, cancellationToken: cancellationToken);
        }
    }
}
