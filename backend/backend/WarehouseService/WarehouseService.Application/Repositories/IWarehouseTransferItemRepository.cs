using WarehouseService.Domain.Entities;

namespace WarehouseService.Application.Repositories;

public interface IWarehouseTransferItemRepository
{
    Task<WarehouseTransferItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<WarehouseTransferItem> GetQueryable();
    Task AddAsync(WarehouseTransferItem item, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<WarehouseTransferItem> items, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IQueryable<WarehouseItem> GetWarehouseItemQueryable();
}