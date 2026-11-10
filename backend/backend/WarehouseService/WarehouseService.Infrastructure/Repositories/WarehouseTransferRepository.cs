using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Infrastructure.DbContext;

namespace WarehouseService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho WarehouseTransfer CRUD.
/// Sử dụng WarehouseDbContext để truy cập DB.
/// </summary>
public class WarehouseTransferRepository : IWarehouseTransferRepository
{
    private readonly WarehouseDbContext _dbContext;

    public WarehouseTransferRepository(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Lấy 1 warehouse transfer chưa bị xóa theo ID.</summary>
    public async Task<WarehouseTransfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WarehouseTransfers
            .FirstOrDefaultAsync(wt => wt.Id == id && wt.IsDeleted == false, cancellationToken);
    }

    /// <summary>Kiểm tra TransferCode đã tồn tại chưa (loại trừ 1 ID nếu cần).</summary>
    public async Task<bool> CodeExistsAsync(string transferCode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.WarehouseTransfers
            .Where(wt => wt.TransferCode == transferCode && wt.IsDeleted == false);

        if (excludeId.HasValue)
            query = query.Where(wt => wt.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>Thêm warehouse transfer mới vào DB.</summary>
    public async Task AddAsync(WarehouseTransfer warehouseTransfer, CancellationToken cancellationToken = default)
    {
        await _dbContext.WarehouseTransfers.AddAsync(warehouseTransfer, cancellationToken);
    }

    /// <summary>Trả về IQueryable để query linh hoạt.</summary>
    public IQueryable<WarehouseTransfer> GetQueryable()
    {
        return _dbContext.WarehouseTransfers.AsQueryable();
    }

    /// <summary>Lưu thay đổi vào DB.</summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}