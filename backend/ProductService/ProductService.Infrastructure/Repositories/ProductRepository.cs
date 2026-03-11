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
    /// <summary>
    /// Gets all products asynchronous.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns></returns>
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
    /// <summary>
    /// Gets all product with paging by asynchronous.
    /// </summary>
    /// <param name="keyword">The keyword.</param>
    /// <param name="categoryId">The category identifier.</param>
    /// <param name="pageIndex">Index of the page.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
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
    /// <summary>
    /// Gets the product by identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants
                .Where(pv => pv.IsDeleted == false))
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                // p.IsActive == true &&
                p.IsDeleted == false);
    }
    /// <summary>
    /// Gets the product by name asynchronous.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="excludeId">The exclude identifier.</param>
    /// <returns></returns>
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
    /// <summary>
    /// Gets the product by name containing asynchronous.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
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
    /// <summary>
    /// Get product by variant SKU, including Category and active Variants.
    /// </summary>
    /// <param name="sku"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Get multiple products by variant SKUs, including Category and active Variants.
    /// </summary>
    /// <param name="skus"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Updates the with variants only asynchronous.
    /// </summary>
    /// <param name="updatedProduct">The updated product.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Không tìm thấy sản phẩm Id: {updatedProduct.Id}</exception>
    public async Task<Product> UpdateWithVariantsOnlyAsync(Product updatedProduct)
    {
        // 1. Fetch: Lấy dữ liệu kèm theo Variants hiện tại
        var existingProduct = await _dbContext.Products
            .Include(p => p.ProductVariants)
            .FirstOrDefaultAsync(p => p.Id == updatedProduct.Id)
            ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm Id: {updatedProduct.Id}");

        // 2. Map Product Fields: Chỉ map những trường cần thiết
        existingProduct.Name = updatedProduct.Name;
        existingProduct.Description = updatedProduct.Description;
        existingProduct.CategoryId = updatedProduct.CategoryId;
        existingProduct.IsActive = updatedProduct.IsActive;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        // 3. Sync Variants: Xử lý danh sách con
        var currentVariants = existingProduct.ProductVariants.ToList();
        var newVariants = updatedProduct.ProductVariants;

        // --- A. Xóa (Delete): Những Sku có trong DB nhưng không có trong request ---
        foreach (var oldV in currentVariants)
        {
            if (!newVariants.Any(nv => nv.Sku == oldV.Sku))
            {
                _dbContext.ProductVariants.Remove(oldV);
            }
        }

        // --- B. Cập nhật & Thêm mới (Update & Add) ---
        foreach (var newV in newVariants)
        {
            var existingV = currentVariants.FirstOrDefault(v => v.Sku == newV.Sku);

            if (existingV != null)
            {
                // Map thủ công từng field cho Variant đã tồn tại
                existingV.Name = newV.Name;
                existingV.Attributes = newV.Attributes;
                existingV.BasePrice = newV.BasePrice;
                existingV.Cost = newV.Cost;
                existingV.IsActive = newV.IsActive;
                // Không gán Id hay ProductId ở đây để tránh lỗi tracking
            }
            else
            {
                // Thêm mới hoàn toàn
                existingProduct.ProductVariants.Add(new ProductVariant
                {
                    Sku = newV.Sku,
                    Name = newV.Name,
                    Attributes = newV.Attributes,
                    BasePrice = newV.BasePrice,
                    Cost = newV.Cost,
                    IsActive = newV.IsActive
                });
            }
        }

        return existingProduct;
    }

    public async Task UpdateWithVariantsAsync(Product updatedProduct)
    {
        await UpdateWithVariantsOnlyAsync(updatedProduct);
        await _dbContext.SaveChangesAsync();
    }
}