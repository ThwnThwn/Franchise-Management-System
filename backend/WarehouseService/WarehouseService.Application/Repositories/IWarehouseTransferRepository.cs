using WarehouseService.Domain.Entities;

namespace WarehouseService.Application.Repositories;

public interface IWarehouseTransferRepository
{
    /// <summary>Lấy 1 warehouse transfer chưa bị xóa theo ID.</summary>
    Task<WarehouseTransfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Kiểm tra TransferCode đã tồn tại chưa (loại trừ 1 ID nếu cần).</summary>
    Task<bool> CodeExistsAsync(string transferCode, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>Thêm warehouse transfer mới vào DB.</summary>
    Task AddAsync(WarehouseTransfer warehouseTransfer, CancellationToken cancellationToken = default);

    /// <summary>Trả về IQueryable để query linh hoạt.</summary>
    IQueryable<WarehouseTransfer> GetQueryable();

    /// <summary>Lưu thay đổi vào DB.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}