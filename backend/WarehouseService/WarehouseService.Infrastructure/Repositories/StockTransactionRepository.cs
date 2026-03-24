using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Infrastructure.DbContext;

namespace WarehouseService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho StockTransaction CRUD.
/// Sử dụng WarehouseDbContext để truy cập DB.
/// </summary>
public class StockTransactionRepository : IStockTransactionRepository
{
    private readonly WarehouseDbContext _dbContext;

    public StockTransactionRepository(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Lấy 1 stock transaction chưa bị xóa theo ID.</summary>
    public async Task<StockTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockTransactions
            .FirstOrDefaultAsync(st => st.Id == id && st.IsDeleted == false, cancellationToken);
    }

    /// <summary>Kiểm tra TransactionCode đã tồn tại chưa (loại trừ 1 ID nếu cần).</summary>
    public async Task<bool> CodeExistsAsync(string transactionCode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StockTransactions
            .Where(st => st.TransactionCode == transactionCode && st.IsDeleted == false);

        if (excludeId.HasValue)
            query = query.Where(st => st.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>Thêm stock transaction mới vào DB.</summary>
    public async Task AddAsync(StockTransaction stockTransaction, CancellationToken cancellationToken = default)
    {
        await _dbContext.StockTransactions.AddAsync(stockTransaction, cancellationToken);
    }

    /// <summary>Trả về IQueryable để query linh hoạt.</summary>
    public IQueryable<StockTransaction> GetQueryable()
    {
        return _dbContext.StockTransactions.AsQueryable();
    }

    /// <summary>Lưu thay đổi vào DB.</summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<List<StockTransaction>> GetByTransferIdAsync(Guid transferId, CancellationToken cancellationToken)
    {
        return _dbContext.StockTransactions
            .Where(st => st.Id == transferId && st.IsDeleted == false)
            .ToListAsync(cancellationToken);
    }
}