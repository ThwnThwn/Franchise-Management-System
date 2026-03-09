using System;
using System.Threading.Tasks;

namespace SalesService.Application.ClientInterface.Products;

public interface IProductServiceClient
{
    /// <summary>
    ///  Get product variants by SKU.
    /// </summary>
    /// <param name="sku"></param>
    /// <returns></returns>
    Task<ProductVariantInfo?> GetProductVariantBySkuAsync(string sku);


    /// <summary>
    /// Get multiple product variants by list of SKUs in one call.
    /// Returns list of variants found (only active variants that exist in requested SKU list).
    /// </summary>
    Task<List<ProductVariantInfo>> GetListProductVariantsBySkusAsync(List<string> skus);

    /// <summary>
    /// Get product SKUs by Category ID.
    /// </summary>
    Task<List<string>> GetSkusByCategoryAsync(Guid categoryId);
}

public class ProductVariantInfo
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public string ProductName { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public string? Attributes { get; set; }
    public decimal? BasePrice { get; set; }
    public string? ImageUrl { get; set; }
}