using InventoryService.Application.AppExceptions;
using InventoryService.Application.Dtos.StoreIngredient;
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
    public class StoreIngredientRepository : GenericRepository<StoreIngredient>, IStoreIngredientRepository
    {
        public StoreIngredientRepository(IInventoryDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<StoreIngredient>> GetStockByStoreIdAndIngredientIdsAsync(Guid storeId, IEnumerable<Guid> ingredientIds, CancellationToken cancellationToken)
        {
            return await _dbContext.StoreIngredients.AsQueryable()
                        .Where(e => e.StoreId == storeId 
                                && ingredientIds.Contains(e.IngredientId) 
                                && e.IsDeleted == false)
                        .ToListAsync(cancellationToken);
        }

        public async Task IncreaseStockAsync(Guid storeId, IEnumerable<IngredientStockItem> items)
        {
            try
            {
                var ids = items.Select(s => s.IngredientId).ToList();
                var ingredientInStore = await _dbContext.StoreIngredients.AsQueryable().Where(e => e.StoreId == storeId
                                                                                        && ids.Contains(e.IngredientId)
                                                                                        && e.IsDeleted == false).ToArrayAsync();
                var existingIngredientIds = ingredientInStore.Select(e => e.IngredientId).ToHashSet();
                var itemShouldAdd = items.Where(it => !existingIngredientIds.Contains(it.IngredientId)).ToList();

                //increase
                foreach (var increaseItem in items)
                {
                    var dbItem = ingredientInStore.FirstOrDefault(s => s.IngredientId == increaseItem.IngredientId);
                    if (dbItem != null)
                    {
                        dbItem.Quantity += increaseItem.Quantity;
                    }
                }

                //add
                if (itemShouldAdd != null)
                {
                    var addList = new List<StoreIngredient>();
                    foreach (var item in itemShouldAdd)
                    {
                        addList.Add(new StoreIngredient
                        {
                            Id = new Guid(),
                            IngredientId = item.IngredientId,
                            Quantity = item.Quantity,
                            MinThreshold = 0,
                            StoreId = storeId
                        });
                    }
                    await _dbContext.StoreIngredients.AddRangeAsync(addList);
                }
                await _dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw new Application.AppExceptions.DbUpdateException("Lỗi update db");
            }
        }
    }
}
