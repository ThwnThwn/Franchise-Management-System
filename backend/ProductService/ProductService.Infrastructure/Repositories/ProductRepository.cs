using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.ApplicationDbContext;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IProductDbContext _context;

    public ProductRepository(IProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllProductsAsync(Guid? categoryId = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants.Where(pv => pv.IsActive == true && pv.IsDeleted == false))
            .Where(p => p.IsActive == true && p.IsDeleted == false);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants.Where(pv => pv.IsActive == true && pv.IsDeleted == false))
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive == true && p.IsDeleted == false);
    }
}