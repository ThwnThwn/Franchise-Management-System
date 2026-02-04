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
}