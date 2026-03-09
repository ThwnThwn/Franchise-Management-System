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
        var rawUrl = configuration["PRODUCT_SERVICE_URL"] ?? "http://localhost:5002";
        // Cho phép env var values like "localhost:5002" bằng cách chuẩn hóa thành URI tuyệt đối.
        _productServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                             rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? rawUrl
            : $"http://{rawUrl}";

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
            var request = new GetProductBySkuRequest { Sku = sku };
            var response = await _client.GetProductBySkuAsync(request);

            if (!response.Found)
            {
                return null;
            }

            return new ProductVariantInfo
            {
                ProductId = Guid.Parse(response.ProductId),
                VariantId = Guid.Parse(response.VariantId),
                ProductName = response.ProductName,
                Sku = response.Sku,
                Attributes = response.Attributes,
                BasePrice = (decimal)response.BasePrice,
                ImageUrl = response.ImageUrl
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling ProductService ({_productServiceUrl}): {ex.Message}");
            throw;
        }
    }

    public async Task<List<ProductVariantInfo>> GetListProductVariantsBySkusAsync(List<string> skus)
    {
        if (skus == null || !skus.Any())
        {
            return new List<ProductVariantInfo>();
        }

        try
        {
            var uniqueSkus = skus.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            if (!uniqueSkus.Any())
            {
                return new List<ProductVariantInfo>();
            }

            var request = new GetProductsBySkusRequest();
            request.Skus.AddRange(uniqueSkus);

            var response = await _client.GetProductsBySkusAsync(request);

            return response.Variants.Select(v => new ProductVariantInfo
            {
                ProductId = Guid.Parse(v.ProductId),
                VariantId = Guid.Parse(v.VariantId),
                ProductName = v.ProductName,
                Sku = v.Sku,
                Attributes = v.Attributes,
                BasePrice = (decimal)v.BasePrice,
                ImageUrl = v.ImageUrl
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling ProductService ({_productServiceUrl}): {ex.Message}");
            throw;
        }
    }

    public async Task<List<string>> GetSkusByCategoryAsync(Guid categoryId)
    {
        try
        {
            var request = new GetSkusByCategoryRequest { CategoryId = categoryId.ToString() };
            var response = await _client.GetSkusByCategoryAsync(request);

            return response.Skus.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling ProductService for category skus ({_productServiceUrl}): {ex.Message}");
            throw;
        }
    }
}