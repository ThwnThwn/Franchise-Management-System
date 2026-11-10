using System;
using ProductService.Application.Helpers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.ApplicationDbContext;

namespace ProductService.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(IProductDbContext dbContext) : base(dbContext)
        {
        }
        /// <summary>
        /// Gets all categories asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<Category>> GetAllCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsActive == true && c.IsDeleted == false)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
        /// <summary>
        /// Gets all asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Category>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsActive == true && c.IsDeleted == false)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        /// <summary>
        /// Gets the by category identifier asynchronous.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Category?> GetByCategoryIdAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsDeleted == false,
                    cancellationToken);
        }
        /// <summary>
        /// Gets the by category name asynchronous.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Category?> GetByCategoryNameAsync(
        string categoryName,
        CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return null;

            var normalizedSearchName = VietnameseHelper.RemoveVietnameseSigns(categoryName.Trim()).ToLower();

            var categories = await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsDeleted == false)
                .ToListAsync(cancellationToken);

            return categories.FirstOrDefault(c =>
                VietnameseHelper.RemoveVietnameseSigns(c.Name.Trim()).ToLower() == normalizedSearchName
            );
        }
        /// <summary>
        /// Gets the by category name containing asynchronous.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Category?> GetByCategoryNameContainingAsync(
            string categoryName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return null;

            var normalizedSearchName = VietnameseHelper.RemoveVietnameseSigns(categoryName.Trim()).ToLower();

            var categories = await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsDeleted == false)
                .ToListAsync(cancellationToken);

            return categories.FirstOrDefault(c =>
                VietnameseHelper.RemoveVietnameseSigns(c.Name).ToLower().Contains(normalizedSearchName)
            );
        }
        /// <summary>
        /// Gets the by identifier with products asynchronous.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Category?> GetByIdWithProductsAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Include(c => c.Products.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsDeleted == false,
                    cancellationToken);
        }
        /// <summary>
        /// Existses the by name asynchronous.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="excludeCategoryId">The exclude category identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<bool> ExistsByNameAsync(
            string categoryName,
            Guid? excludeCategoryId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return false;

            var query = _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.Name.ToLower() == categoryName.ToLower() && c.IsDeleted == false);

            if (excludeCategoryId.HasValue)
                query = query.Where(c => c.Id != excludeCategoryId.Value);

            return await query.AnyAsync(cancellationToken);
        }
        /// <summary>
        /// Determines whether [has products asynchronous] [the specified category identifier].
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   <c>true</c> if [has products asynchronous] [the specified category identifier]; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> HasProductsAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(p => p.CategoryId == categoryId && p.IsDeleted == false,
                    cancellationToken);
        }
    }
}