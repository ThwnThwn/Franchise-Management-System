using InventoryService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Persistences.Repositories
{
    public interface IRecipeRepository : IGenericRepository<Recipe>
    {
        public Task<IEnumerable<Recipe>> GetRecipesByVariantIdAsync(Guid variantId, CancellationToken cancellationToken);
        public Task<IEnumerable<Recipe>?> GetRecipesByVariantIdsAsync(IEnumerable<Guid> variantIds, CancellationToken cancellationToken);
        Task StrictDelete(Recipe entity);
    }
}
