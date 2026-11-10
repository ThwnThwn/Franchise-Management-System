using Grpc.Net.Client;
using ProductService.API;
using SalesService.Application.ClientInterface.Products;

namespace SalesService.API.Services.Products;

public class ProductServiceClient(ProductGrpc.ProductGrpcClient client) : IProductServiceClient
{

    public async Task<ProductVariantInfo?> GetProductVariantBySkuAsync(string sku)
    {
        try
        {
            var request = new GetProductBySkuRequest { Sku = sku };
            var response = await client.GetProductBySkuAsync(request);

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
            Console.WriteLine($"Error calling ProductService: {ex.Message}");
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

            var response = await client.GetProductsBySkusAsync(request);

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
            Console.WriteLine($"Error calling ProductService: {ex.Message}");
            throw;
        }
    }

    public async Task<List<string>> GetSkusByCategoryAsync(Guid categoryId)
    {
        try
        {
            var request = new GetSkusByCategoryRequest { CategoryId = categoryId.ToString() };
            var response = await client.GetSkusByCategoryAsync(request);

            return response.Skus.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling ProductService for category skus: {ex.Message}");
            throw;
        }
    }

    public async Task<List<ProductInfo>> GetProductsByCategoryAsync(Guid categoryId)
    {
        try
        {
            var request = new GetProductsRequest { CategoryId = categoryId.ToString() };
            var response = await client.GetProductsAsync(request);

            return response.Products.Select(p => new ProductInfo
            {
                ProductId = Guid.Parse(p.Id),
                ProductName = p.Name,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                CategoryId = Guid.TryParse(p.CategoryId, out var catId) ? catId : Guid.Empty,
                CategoryName = p.CategoryName,
                IsActive = p.IsActive
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling ProductService GetProductsByCategory: {ex.Message}");
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    public async Task<List<ProductInfo>> GetProductsByIdsAsync(List<Guid> productIds)
    {
        if (productIds == null || !productIds.Any())
            return new List<ProductInfo>();

        try
        {
            var request = new GetProductsByIdsRequest();
            request.ProductIds.AddRange(productIds.Select(id => id.ToString()));

            var response = await client.GetProductsByIdsAsync(request);

            return response.Products.Select(p => new ProductInfo
            {
                ProductId = Guid.Parse(p.Id),
                ProductName = p.Name,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                CategoryId = Guid.TryParse(p.CategoryId, out var catId) ? catId : Guid.Empty,
                CategoryName = p.CategoryName,
                IsActive = p.IsActive
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling ProductService GetProductsByIds: {ex.Message}");
            throw;
        }
    }
}