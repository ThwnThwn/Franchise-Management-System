using ProductService.Domain.Entities;

namespace ProductService.Application.Repositories;

public interface IProductVariantRepository : IGenericRepository<ProductVariant>
{
    Task<ProductVariant?> GetProductVariantByIdAsync(Guid id);
}
