using BuildingBlocks.Core.Pagination;
using InventoryService.Application.Constants;
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

        Task<PagedResult<StoreIngredient>> GetPagedStockAsync(IEnumerable<Guid>? storeIds, IEnumerable<Guid>? categoryIds, StockStatus? stockStatus, IEnumerable<Guid>? ingredientIds, int pageIndex, int pageSize, CancellationToken cancellationToken);
        Task<DecreaseStockResult?> DecreaseStockAtomicallyAsync(Guid storeId, Guid ingredientId, decimal amount);
        
        
        // Queue operations
        Task AddToQueueAsync(IngredientQueue queueItem);
        Task AddToQueueBatchAsync(IEnumerable<IngredientQueue> queueItems);
        Task RemoveQueueByOrderIdAsync(Guid orderId);
        Task<decimal> GetReservedQuantityAsync(Guid storeId, Guid ingredientId);
        Task<List<IngredientQueue>> GetQueueByOrderIdAsync( Guid orderId,CancellationToken cancellationToken);
    }
}
