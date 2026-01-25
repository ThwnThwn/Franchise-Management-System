using System;
using System.Threading.Tasks;

namespace SaleService.Application.Interfaces;

public interface IProductServiceClient
{
    Task<ProductVariantInfo?> GetProductVariantBySkuAsync(string sku);
}

public class ProductVariantInfo
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public string ProductName { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public decimal? BasePrice { get; set; }
    public string? ImageUrl { get; set; }
}
