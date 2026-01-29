using Grpc.Net.Client;
using ProductService.API;
using SalesService.Application.ClientInterface.Products;

namespace SalesService.API.Services.Products;

public class ProductServiceClient : IProductServiceClient
{
    private readonly ProductGrpc.ProductGrpcClient _client;
    private readonly string _productServiceUrl;

    public ProductServiceClient(IConfiguration configuration)
    {
        var rawUrl = configuration["PRODUCT_SERVICE_URL"] ?? "https://localhost:5002";
        // Allow env var values like "localhost:5002" by normalizing to an absolute URI.
        _productServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                             rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? rawUrl
            : $"https://{rawUrl}";

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var channel = GrpcChannel.ForAddress(_productServiceUrl, new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        _client = new ProductGrpc.ProductGrpcClient(channel);
    }

    public async Task<ProductVariantInfo?> GetProductVariantBySkuAsync(string sku)
    {
        try
        {
            var request = new GetProductsRequest { CategoryId = string.Empty };
            var response = await _client.GetProductsAsync(request);

            foreach (var product in response.Products)
            {
                var variant = product.Sizes.FirstOrDefault(s => s.Sku == sku && s.IsActive);
                if (variant != null)
                {
                    return new ProductVariantInfo
                    {
                        ProductId = Guid.Parse(product.Id),
                        VariantId = Guid.Parse(variant.Id),
                        ProductName = product.Name,
                        Sku = variant.Sku,
                        BasePrice = (decimal)variant.BasePrice,
                        ImageUrl = product.ImageUrl
                    };
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling ProductService ({_productServiceUrl}): {ex.Message}");
            throw;
        }
    }
}