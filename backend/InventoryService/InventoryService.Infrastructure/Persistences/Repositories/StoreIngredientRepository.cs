using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using InventoryService.Application.AppExceptions;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryService.Application.Abstractions;

namespace InventoryService.Infrastructure.Persistences.Repositories
{
    /// <summary>
    /// Repository quản lý kho nguyên liệu của cửa hàng.
    /// Chứa các logic nhập kho, trừ kho, và quản lý hàng đợi giữ hàng (Reservation Queue).
    /// </summary>
    public class StoreIngredientRepository : GenericRepository<StoreIngredient>, IStoreIngredientRepository
    {
        public StoreIngredientRepository(IInventoryDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Lấy thông tin tồn kho cho danh sách nguyên liệu của một cửa hàng cụ thể.
        /// (Từ nhánh HEAD - Feature Inventory Deduction)
        /// </summary>
        public async Task<IEnumerable<StoreIngredient>> GetStockByStoreIdAndIngredientIdsAsync(Guid storeId, IEnumerable<Guid> ingredientIds, CancellationToken cancellationToken)
        {
            return await _dbContext.StoreIngredients.AsQueryable()
                        .Include(e => e.Ingredient)
                        .Where(e => e.StoreId == storeId 
                                && ingredientIds.Contains(e.IngredientId) 
                                && e.IsDeleted == false)
                        .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lấy thông tin tồn kho có phân trang và lọc điều kiện.
        /// (Từ nhánh Main - Feature Category/Refactor)
        /// </summary>
        public async Task<PagedResult<StoreIngredient>> GetPagedStockAsync(IEnumerable<Guid>? storeIds, IEnumerable<Guid>? categoryIds, StockStatus? stockStatus, IEnumerable<Guid>? ingredientIds, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            var query = _dbContext.StoreIngredients.AsQueryable()
                        .Include(e => e.Ingredient)
                        .ThenInclude(e => e.IngredientCategory)
                        .Where(e => e.IsDeleted == false)
                        .AsNoTracking();
            if(storeIds != null && storeIds.Any())
            {
                query = query.Where(e => storeIds.Contains(e.StoreId));
            }
            if (categoryIds != null && categoryIds.Any()) { 
                query = query.Where(e => e.Ingredient.CategoryId != null && categoryIds.Contains((Guid)e.Ingredient.CategoryId));
            }
            if (ingredientIds != null && ingredientIds.Any())
            {
                query = query.Where(e => ingredientIds.Contains(e.IngredientId));
            }
            if (stockStatus != null)
            {
                switch (stockStatus)
                {
                    case StockStatus.Sufficient:
                        query = query.Where(e => e.Quantity > e.MinThreshold);
                        break;
                    case StockStatus.LowStock:
                        query = query.Where(e => e.Quantity <= e.MinThreshold && e.Quantity > 0);
                        break;
                    case StockStatus.OutOfStock:
                        query = query.Where(e => e.Quantity == 0);
                        break;
                    default:
                        break;
                }
            }
            return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
        }

        /// <summary>
        /// Tăng số lượng tồn kho (Nhập kho).
        /// </summary>
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

        /// <summary>
        /// Trừ tồn kho một cách nguyên tử (Atomic).
        /// </summary>
        public async Task<DecreaseStockResult?> DecreaseStockAtomicallyAsync(Guid storeId, Guid ingredientId, decimal amount)
        {
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            
            command.CommandText = @"
                UPDATE ""store_ingredients""
                SET ""quantity"" = ""quantity"" - @amount,
                    ""updated_at"" = NOW()
                WHERE ""store_id"" = @storeId
                  AND ""ingredient_id"" = @ingredientId
                  AND ""quantity"" >= @amount
                RETURNING ""quantity"", ""min_threshold"";
            ";

            var pAmount = command.CreateParameter();
            pAmount.ParameterName = "@amount";
            pAmount.Value = amount;
            command.Parameters.Add(pAmount);

            var pStoreId = command.CreateParameter();
            pStoreId.ParameterName = "@storeId";
            pStoreId.Value = storeId;
            command.Parameters.Add(pStoreId);

            var pIngId = command.CreateParameter();
            pIngId.ParameterName = "@ingredientId";
            pIngId.Value = ingredientId;
            command.Parameters.Add(pIngId);

            if (_dbContext.Database.CurrentTransaction != null)
            {
                command.Transaction = _dbContext.Database.CurrentTransaction.GetDbTransaction();
            }

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new DecreaseStockResult
                {
                    Quantity = reader.GetDecimal(0),
                    MinThreshold = reader.IsDBNull(1) ? null : reader.GetDecimal(1)
                };
            }

            return null; // Update thất bại
        }

        /// <summary>
        /// Thêm yêu cầu giữ hàng vào hàng đợi.
        /// </summary>
        public async Task AddToQueueAsync(IngredientQueue queueItem)
        {
            await _dbContext.IngredientQueues.AddAsync(queueItem);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Thêm nhiều yêu cầu giữ hàng vào hàng đợi cùng lúc (Batch Insert).
        /// Giúp tối ưu hiệu năng Database (chỉ mở kết nối 1 lần).
        /// </summary>
        /// <param name="queueItems">Danh sách các yêu cầu cần thêm.</param>
        public async Task AddToQueueBatchAsync(IEnumerable<IngredientQueue> queueItems)
        {
            await _dbContext.IngredientQueues.AddRangeAsync(queueItems);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa queue theo OrderId.
        /// </summary>
        public async Task RemoveQueueByOrderIdAsync(Guid orderId)
        {
            var items = await _dbContext.IngredientQueues
                .Where(x => x.OrderId == orderId)
                .ToListAsync();

            if (items.Any())
            {
                _dbContext.IngredientQueues.RemoveRange(items);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Lấy tổng số lượng đang Reserved.
        /// </summary>
        public async Task<decimal> GetReservedQuantityAsync(Guid storeId, Guid ingredientId)
        {
            return await _dbContext.IngredientQueues
                .Where(x => x.StoreId == storeId 
                         && x.IngredientId == ingredientId 
                         && x.ExpiresAt > DateTimeOffset.UtcNow 
                         && x.IsDeleted == false)
                .SumAsync(x => x.Amount);
        }

        public async Task<List<IngredientQueue>> GetQueueByOrderIdAsync(Guid orderId,CancellationToken cancellationToken)
        {
            return await _dbContext.IngredientQueues
                .Where(x =>
                    x.OrderId == orderId &&
                    x.ExpiresAt > DateTimeOffset.UtcNow &&
                    x.IsDeleted == false)
                .ToListAsync(cancellationToken);
        }
    }
}
