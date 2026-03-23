using WarehouseService.Domain.Entities;

namespace WarehouseService.Application.Repositories;

public interface IStockTransactionRepository
{
    /// <summary>Lấy 1 stock transaction chưa bị xóa theo ID.</summary>
    Task<StockTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Kiểm tra TransactionCode đã tồn tại chưa (loại trừ 1 ID nếu cần).</summary>
    Task<bool> CodeExistsAsync(string transactionCode, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>Thêm stock transaction mới vào DB.</summary>
    Task AddAsync(StockTransaction stockTransaction, CancellationToken cancellationToken = default);

    /// <summary>Trả về IQueryable để query linh hoạt.</summary>
    IQueryable<StockTransaction> GetQueryable();

    /// <summary>Lưu thay đổi vào DB.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<List<StockTransaction>> GetByTransferIdAsync(Guid transferId, CancellationToken cancellationToken);
}