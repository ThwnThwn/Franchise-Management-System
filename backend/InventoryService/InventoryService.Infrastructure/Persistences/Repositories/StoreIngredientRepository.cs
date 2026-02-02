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
    public class StoreIngredientRepository : GenericRepository<StoreIngredient>, IStoreIngredientRepository
    {
        public StoreIngredientRepository(IInventoryDbContext dbContext) : base(dbContext)
        {
        }

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

        public async Task<IEnumerable<StoreIngredient>> GetStockByStoreIdAndIngredientIdsAsync(Guid storeId, IEnumerable<Guid> ingredientIds, CancellationToken cancellationToken)
        {
            return await _dbContext.StoreIngredients.AsQueryable()
                        .Include(e => e.Ingredient)
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
        /// <summary>
        /// Trừ tồn kho một cách nguyên tử (Atomic) để đảm bảo an toàn đồng thời (concurrency).
        /// Sử dụng câu lệnh SQL trực tiếp với điều kiện `quantity >= amount` để chặn race condition ở cấp database.
        /// </summary>
        /// <param name="storeId">ID cửa hàng</param>
        /// <param name="ingredientId">ID nguyên liệu</param>
        /// <param name="amount">Số lượng cần trừ</param>
        /// <returns>
        /// Trả về <see cref="DecreaseStockResult"/> chứa số lượng tồn mới và ngưỡng cảnh báo nếu trừ thành công.
        /// Trả về <c>null</c> nếu không đủ tồn kho (cập nhật thất bại do điều kiện WHERE không thỏa mãn).
        /// </returns>
        public async Task<DecreaseStockResult?> DecreaseStockAtomicallyAsync(Guid storeId, Guid ingredientId, decimal amount)
        {
            // Sử dụng ADO.NET command bên dưới để lấy kết quả trả về từ mệnh đề RETURNING
            // Lưu ý: FromSqlRaw của EF Core yêu cầu phải định nghĩa một keyless Entity trong DbSet, 
            // điều mà ta muốn tránh đối với một DTO đơn giản như kết quả này.
            
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            
            // Thiết lập tham số rõ ràng (Parameterized Query) để tránh lỗi SQL injection.
            // Dùng @parameter thay vì cộng chuỗi trực tiếp.

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

            // Quan trọng: Nếu đang có EF Core Transaction bao quanh, 
            // ta phải gán transaction đó cho command này để nó chạy cùng một nhịp (Atomic).
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

            return null; // Update thất bại (điều kiện WHERE không thỏa -> không đủ tồn kho)
        }
    }
}
