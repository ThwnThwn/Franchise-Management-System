using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Persistences.Repositories
{
    public interface IStoreIngredientRepository : IGenericRepository<StoreIngredient>
    {
        Task IncreaseStockAsync(Guid storeId, IEnumerable<IngredientStockItem> items);
        Task<IEnumerable<StoreIngredient>> GetStockByStoreIdAndIngredientIdsAsync(Guid storeId, IEnumerable<Guid> ingredientIds, CancellationToken cancellationToken);
    }
}
