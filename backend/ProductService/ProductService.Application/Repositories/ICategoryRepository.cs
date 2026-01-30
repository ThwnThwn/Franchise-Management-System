
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Category?> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<Category?> GetByCategoryNameAsync(string categoryName, CancellationToken cancellationToken = default);
        Task<Category?> GetByCategoryNameContainingAsync(string categoryName, CancellationToken cancellationToken = default);
        Task<Category?> GetByIdWithProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string categoryName, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default);
        Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
    }
}
