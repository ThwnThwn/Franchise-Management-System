using WarehouseService.Domain.Entities;

namespace WarehouseService.Application.Repositories;

public interface IWarehouseRepository
{
    /// <summary>Lấy 1 warehouse chưa bị xóa theo ID.</summary>
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Kiểm tra Code đã tồn tại chưa (loại trừ 1 ID nếu cần).</summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>Thêm warehouse mới vào DB.</summary>
    Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default);

    /// <summary>Trả về IQueryable để query linh hoạt.</summary>
    IQueryable<Warehouse> GetQueryable();

    /// <summary>Lưu thay đổi vào DB.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistWithStoreId(Guid storeId, CancellationToken cancellationToken = default);
    Task<bool> ExistWithStoreAddress(string address, CancellationToken cancellationToken = default);
}