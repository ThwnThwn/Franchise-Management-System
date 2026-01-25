using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProductService.API;
using SaleService.Application.Interfaces.Products;

namespace SaleService.API.Services.Products;

public class ProductServiceClient : IProductServiceClient
{
    private readonly ProductGrpc.ProductGrpcClient _client;
    private readonly string _productServiceUrl;

    public ProductServiceClient(IConfiguration configuration)
    {
        _productServiceUrl = configuration["PRODUCT_SERVICE_URL"]
            ?? "https://localhost:5002";

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
            Console.WriteLine($"Error calling ProductService: {ex.Message}");
            throw;
        }
    }
}
