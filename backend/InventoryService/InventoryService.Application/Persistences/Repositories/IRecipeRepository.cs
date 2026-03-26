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
        //Task StrictDelete(Recipe entity);
        // Lấy danh sách VariantId duy nhất có sử dụng Ingredient này
        public Task<List<Guid>> GetVariantIdsByIngredientIdAsync(Guid ingredientId, CancellationToken ct);

        // Lấy toàn bộ Recipe của 1 Variant (phải kèm theo thông tin Ingredient để lấy Cost)
        public Task<List<Recipe>> GetRecipesByVariantIdWithIngredientAsync(Guid variantId, CancellationToken ct);
    }
}
