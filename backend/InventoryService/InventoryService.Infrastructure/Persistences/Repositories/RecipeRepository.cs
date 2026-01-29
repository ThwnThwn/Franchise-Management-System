using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Infrastructure.Persistences.Repositories
{
    public class RecipeRepository : GenericRepository<Recipe>, IRecipeRepository
    {
        public RecipeRepository(IInventoryDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByVariantIdAsync(Guid variantId, CancellationToken cancellationToken)
        {
            return await _dbContext.Recipes.AsQueryable()
                .Where(r => r.VariantId == variantId && r.IsDeleted == false)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Recipe>?> GetRecipesByVariantIdsAsync(IEnumerable<Guid> variantIds, CancellationToken cancellationToken)
        {
            return await _dbContext.Recipes.AsQueryable()
                .Where(r => variantIds.Contains(r.VariantId) && r.IsDeleted == false)
                .ToListAsync(cancellationToken)
                .ContinueWith(t => (IEnumerable<Recipe>)t.Result, cancellationToken);
        }
    }
}
