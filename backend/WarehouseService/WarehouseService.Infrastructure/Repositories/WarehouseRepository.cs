using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Infrastructure.DbContext;

namespace WarehouseService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Warehouse CRUD.
/// Sử dụng WarehouseDbContext để truy cập DB.
/// </summary>
public class WarehouseRepository : IWarehouseRepository
{
    private readonly WarehouseDbContext _dbContext;

    public WarehouseRepository(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Lấy 1 warehouse chưa bị xóa theo ID.</summary>
    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && w.IsDeleted == false, cancellationToken);
    }

    /// <summary>Kiểm tra Code đã tồn tại chưa (loại trừ 1 ID nếu cần).</summary>
    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Warehouses
            .Where(w => w.Code == code && w.IsDeleted == false);

        if (excludeId.HasValue)
            query = query.Where(w => w.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>Thêm warehouse mới vào DB.</summary>
    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        await _dbContext.Warehouses.AddAsync(warehouse, cancellationToken);
    }

    /// <summary>Trả về IQueryable để query linh hoạt.</summary>
    public IQueryable<Warehouse> GetQueryable()
    {
        return _dbContext.Warehouses.AsQueryable();
    }

    /// <summary>Lưu thay đổi vào DB.</summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistWithStoreId(Guid storeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Warehouses
            .AnyAsync(w => w.StoreId == storeId && w.IsDeleted == false, cancellationToken);
    }

    public Task<bool> ExistWithStoreAddress(string address, CancellationToken cancellationToken = default)
    {
        return _dbContext.Warehouses
            .AnyAsync(w => w.Address.Equals(address) && w.IsDeleted == false, cancellationToken);
    }
}