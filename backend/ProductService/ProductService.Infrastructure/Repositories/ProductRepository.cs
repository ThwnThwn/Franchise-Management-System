using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Helpers;
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

    public async Task<PagedResult<Product>> GetAllProductWithPagingByAsync(
     string? keyword,
     Guid? categoryId,
     int pageIndex,
     int pageSize,
     CancellationToken cancellationToken)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .Where(p =>
                !p.IsDeleted &&
                (!categoryId.HasValue || p.CategoryId == categoryId)
            );

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var searchTerm = VietnameseHelper.RemoveVietnameseSigns(keyword.Trim()).ToLower();
            searchTerm = searchTerm.Replace("%", "[%]").Replace("_", "[_]");

            // Load data về client
            var allProducts = await query.ToListAsync(cancellationToken);

            // Filter
            var filtered = allProducts
                .Where(p =>
                    VietnameseHelper.RemoveVietnameseSigns(p.Name).ToLower().Contains(searchTerm) ||
                    (p.Category != null && VietnameseHelper.RemoveVietnameseSigns(p.Category.Name).ToLower().Contains(searchTerm)) ||
                    p.ProductVariants.Any(pv => VietnameseHelper.RemoveVietnameseSigns(pv.Sku).ToLower().Contains(searchTerm))
                )
                .OrderByDescending(p => p.CreatedAt);

            return filtered.ToPagedList(pageIndex, pageSize);
        }

        // Nếu không có keyword, xử lý bình thường
        query = query.OrderByDescending(p => p.CreatedAt);
        return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
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

    public async Task<Product?> GetProductByNameAsync(string name, Guid? excludeId)
    {
        var normalizedSearchName = VietnameseHelper.RemoveVietnameseSigns(name.Trim()).ToLower();

        var query = _dbContext.Products
            .Include(p => p.Category)
            .Where(p =>
                p.IsActive == true &&
                p.IsDeleted == false);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        var products = await query.ToListAsync();

        return products.FirstOrDefault(p =>
            VietnameseHelper.RemoveVietnameseSigns(p.Name.Trim()).ToLower() == normalizedSearchName
        );
    }

    public async Task<List<Product>> GetProductByNameContainingAsync(string name)
    {
        var normalizedSearchName = VietnameseHelper.RemoveVietnameseSigns(name.Trim()).ToLower();

        var products = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants
                .Where(pv => pv.IsActive == true && pv.IsDeleted == false))
            .Where(p =>
                p.IsActive == true &&
                p.IsDeleted == false)
            .Take(500) // Giới hạn để tránh load quá nhiều
            .ToListAsync();

        return products
            .Where(p => VietnameseHelper.RemoveVietnameseSigns(p.Name).ToLower().Contains(normalizedSearchName))
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }

    public async Task<Product?> GetProductBySkuAsync(string sku)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants
                .Where(pv => pv.IsActive == true && pv.IsDeleted == false))
            .Where(p => p.IsActive == true && p.IsDeleted == false 
                                            && p.Category.IsActive == true 
                                            && p.Category.IsDeleted == false)
            .FirstOrDefaultAsync(p =>
                p.ProductVariants.Any(pv =>
                    pv.Sku == sku &&
                    pv.IsActive == true &&
                    pv.IsDeleted == false));
    }

    public async Task<List<Product>> GetProductsBySkusAsync(List<string> skus)
    {
        var skuSet = new HashSet<string>(skus, StringComparer.OrdinalIgnoreCase);

        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants
                .Where(pv => pv.IsActive == true && pv.IsDeleted == false))
            .Where(p =>
                p.IsActive == true &&
                p.IsDeleted == false &&
                p.Category.IsActive == true &&
                p.Category.IsDeleted == false &&
                p.ProductVariants.Any(pv =>
                    skuSet.Contains(pv.Sku) &&
                    pv.IsActive == true &&
                    pv.IsDeleted == false))
            .ToListAsync();
    }
}