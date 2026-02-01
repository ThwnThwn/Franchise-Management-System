using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.ApplicationDbContext;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(IProductDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<Product>> GetAllProductsAsync(Guid? categoryId = null)
    {
        var query = _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants.Where(pv => pv.IsActive == true && pv.IsDeleted == false))
            .Where(p => p.IsActive == true && p.IsDeleted == false);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants
                .Where(pv => pv.IsActive == true && pv.IsDeleted == false))
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.IsActive == true &&
                p.IsDeleted == false);
    }

    public async Task<Product?> GetProductByNameAsync(string name, Guid? exId)
    {
        var query = _dbContext.Products
            .Include(p => p.Category)
            .Where(p =>
                p.Name == name &&
                p.IsActive == true &&
                p.IsDeleted == false);

        if (exId.HasValue)
        {
            query = query.Where(p => p.Id != exId.Value);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<Product>> GetProductByNameContainingAsync(string name)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants
                .Where(pv => pv.IsActive == true && pv.IsDeleted == false))
            .Where(p =>
                EF.Functions.Like(p.Name, $"%{name}%") &&
                p.IsActive == true &&
                p.IsDeleted == false)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}