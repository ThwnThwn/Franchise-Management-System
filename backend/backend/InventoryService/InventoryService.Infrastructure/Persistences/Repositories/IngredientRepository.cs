using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using InventoryService.Application.Abstractions;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Infrastructure.Persistences.Repositories
{
    public class IngredientRepository : GenericRepository<Ingredient>, IIngredientRepository
    {
        public IngredientRepository(IInventoryDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<PagedResult<Ingredient>> GetPagedIngredientsAsync(Guid? categoryId, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            var query = _dbContext.Ingredients
                .Include(i => i.IngredientCategory)
                .Where(i => i.IsDeleted == false)
                .AsNoTracking();

            if (categoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == categoryId.Value);
            }
            query = query.OrderByDescending(u => u.CreatedAt);

            return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
        }

        public async Task<List<Ingredient>> GetZeroCostIngredientsAsync(CancellationToken ct)
        {
            // Sử dụng AsNoTracking để tăng tốc độ truy vấn vì ta chỉ cần data để xử lý logic
            // Tuy nhiên, vì sau đó ta cập nhật cost, ta có thể bỏ AsNoTracking 
            // hoặc dùng Attach lại. Ở đây mình để bình thường để EF tự track và Update cho tiện.
            return await _dbContext.Ingredients
                .Where(i => i.Cost == 0 && !i.IsDeleted)
                .ToListAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
