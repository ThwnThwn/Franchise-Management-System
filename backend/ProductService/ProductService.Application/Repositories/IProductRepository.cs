using BuildingBlocks.Core.Pagination;
using ProductService.Domain.Entities;

namespace ProductService.Application.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<List<Product>> GetAllProductsAsync(Guid? categoryId = null);

    Task<Product?> GetProductByIdAsync(Guid id);

    Task<Product?> GetProductByNameAsync(string name, Guid? excludeId);

    Task<List<Product?>> GetProductByNameContainingAsync(string name);

    Task<PagedResult<Product>> GetAllProductWithPagingByAsync(string? keyword, Guid? categoryId, int pageIndex, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Get product by variant SKU, including Category and active Variants.
    /// </summary>
    Task<Product?> GetProductBySkuAsync(string sku);

    /// <summary>
    /// Get multiple products by variant SKUs, including Category and active Variants.
    /// </summary>
    Task<List<Product>> GetProductsBySkusAsync(List<string> skus);

    Task UpdateWithVariantsAsync(Product updatedProduct);

    Task<Product> UpdateWithVariantsOnlyAsync(Product updatedProduct);

    Task<List<Product>> GetProductsByIdsAsync(List<Guid> productIds);
}