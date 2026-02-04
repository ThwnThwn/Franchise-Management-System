using Microsoft.EntityFrameworkCore;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.ApplicationDbContext;

namespace ProductService.Infrastructure.Repositories
{
    public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(IProductDbContext _context) : base(_context) { }

        public async Task<ProductVariant?> GetProductVariantByIdAsync(Guid id)
        {
            return await _dbContext.ProductVariants
                .AsTracking()
                .FirstOrDefaultAsync(pv => pv.Id == id);
        }

    }
}
