using System;
using System.Collections.Generic;
using System.Linq;
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
        // Cho phép env var values like "localhost:5002" bằng cách chuẩn hóa thành URI tuyệt đối.
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

    public async Task<List<ProductVariantInfo>> GetListProductVariantsBySkusAsync(List<string> skus)
    {
        if (skus == null || !skus.Any())
        {
            return new List<ProductVariantInfo>();
        }

        try
        {
            // Xóa duplicates và empty sku
            var uniqueSkus = skus.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            
            if (!uniqueSkus.Any())
            {
                return new List<ProductVariantInfo>();
            }

            // Get All products để check sku có tồn tại không
            var request = new GetProductsRequest { CategoryId = string.Empty };
            var response = await _client.GetProductsAsync(request);

            var result = new List<ProductVariantInfo>();

            // Tạo set của các sku để check xem sku có tồn tại không
            var requestedSkuSet = new HashSet<string>(uniqueSkus, StringComparer.OrdinalIgnoreCase);

            // Duyệt tất cả các sản phẩm 
            foreach (var product in response.Products)
            {
                foreach (var variant in product.Sizes)
                {
                    // Chỉ include active và trong list sku
                    if (variant.IsActive && requestedSkuSet.Contains(variant.Sku))
                    {
                        result.Add(new ProductVariantInfo
                        {
                            ProductId = Guid.Parse(product.Id),
                            VariantId = Guid.Parse(variant.Id),
                            ProductName = product.Name,
                            Sku = variant.Sku,
                            BasePrice = (decimal)variant.BasePrice,
                            ImageUrl = product.ImageUrl
                        });
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling ProductService ({_productServiceUrl}): {ex.Message}");
            throw;
        }
    }
}