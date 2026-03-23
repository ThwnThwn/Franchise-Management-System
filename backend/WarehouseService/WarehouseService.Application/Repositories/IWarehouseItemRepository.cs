using WarehouseService.Domain.Entities;

namespace WarehouseService.Application.Repositories;

public interface IWarehouseItemRepository
{
    /// <summary>Lấy 1 warehouse item chưa bị xóa theo ID.</summary>
    Task<WarehouseItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Kiểm tra ProductId đã tồn tại trong kho chưa (loại trừ 1 ID nếu cần).</summary>
    Task<bool> IngredientExistsInWarehouseAsync(Guid warehouseId, Guid ingredientId, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>Thêm warehouse item mới vào DB.</summary>
    Task AddAsync(WarehouseItem warehouseItem, CancellationToken cancellationToken = default);

    /// <summary>Trả về IQueryable để query linh hoạt.</summary>
    IQueryable<WarehouseItem> GetQueryable();

    /// <summary>Lưu thay đổi vào DB.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}