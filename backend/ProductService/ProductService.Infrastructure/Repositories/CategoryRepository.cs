using System;
using System.Collections.Generic;
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

        public async Task<List<Category>> GetAllCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsActive == true && c.IsDeleted == false)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Category>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsActive == true && c.IsDeleted == false)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Category?> GetByCategoryIdAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsDeleted == false,
                    cancellationToken);
        }

        public async Task<Category?> GetByCategoryNameAsync(
            string categoryName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return null;

            return await _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.Name.ToLower() == categoryName.ToLower() && c.IsDeleted == false,
                    cancellationToken);
        }

        public async Task<Category?> GetByCategoryNameContainingAsync(
            string categoryName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return null;

            return await _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.Name.ToLower().Contains(categoryName.ToLower()) && c.IsDeleted == false,
                    cancellationToken);
        }

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