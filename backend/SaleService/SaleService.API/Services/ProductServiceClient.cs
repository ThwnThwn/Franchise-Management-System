using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProductService.API;
using SaleService.Application.Interfaces;

namespace SaleService.API.Services;

public class ProductServiceClient : IProductServiceClient
{
    private readonly ProductGrpc.ProductGrpcClient _client;
    private readonly string _productServiceUrl;

    public ProductServiceClient(IConfiguration configuration)
    {
        // Lấy URL từ config hoặc environment variable
        _productServiceUrl = configuration["PRODUCT_SERVICE_URL"]
            ?? "https://localhost:5002"; // Default ProductService URL

        // Tạo gRPC channel và client với cấu hình cho development
        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // Chỉ dùng trong development
        };

        var channel = GrpcChannel.ForAddress(_productServiceUrl, new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        _client = new ProductGrpc.ProductGrpcClient(channel);
    }


    /// <summary>
    /// Lấy thông tin product variant theo SKU 
    /// </summary>
    /// <param name="sku"></param>
    /// <returns></returns>
    public async Task<ProductVariantInfo?> GetProductVariantBySkuAsync(string sku)
    {
        try
        {
            // Lấy tất cả products 
            var request = new GetProductsRequest { CategoryId = string.Empty };
            var response = await _client.GetProductsAsync(request);

            // Tìm variant theo SKU trong tất cả products
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

            return null; // Không tìm thấy
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling ProductService: {ex.Message}");
            throw;
        }
    }
}
