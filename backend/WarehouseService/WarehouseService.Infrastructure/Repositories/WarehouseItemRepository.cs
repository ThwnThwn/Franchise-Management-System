using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Infrastructure.DbContext;

namespace WarehouseService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho WarehouseItem CRUD.
/// Sử dụng WarehouseDbContext để truy cập DB.
/// </summary>
public class WarehouseItemRepository : IWarehouseItemRepository
{
    private readonly WarehouseDbContext _dbContext;

    public WarehouseItemRepository(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Lấy 1 warehouse item chưa bị xóa theo ID.</summary>
    public async Task<WarehouseItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WarehouseItems
            .FirstOrDefaultAsync(wi => wi.Id == id && wi.IsDeleted == false, cancellationToken);
    }

    /// <summary>Kiểm tra ProductId đã tồn tại trong kho chưa (loại trừ 1 ID nếu cần).</summary>
    public async Task<bool> IngredientExistsInWarehouseAsync(Guid warehouseId, Guid IngredientId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.WarehouseItems
            .Where(wi => wi.WarehouseId == warehouseId && wi.IngredientId == IngredientId && wi.IsDeleted == false);

        if (excludeId.HasValue)
            query = query.Where(wi => wi.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>Thêm warehouse item mới vào DB.</summary>
    public async Task AddAsync(WarehouseItem warehouseItem, CancellationToken cancellationToken = default)
    {
        await _dbContext.WarehouseItems.AddAsync(warehouseItem, cancellationToken);
    }

    /// <summary>Trả về IQueryable để query linh hoạt.</summary>
    public IQueryable<WarehouseItem> GetQueryable()
    {
        return _dbContext.WarehouseItems.AsQueryable();
    }

    /// <summary>Lưu thay đổi vào DB.</summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}