using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Infrastructure.DbContext;

namespace WarehouseService.Infrastructure.Repositories;

public class WarehouseTransferItemRepository : IWarehouseTransferItemRepository
{
    private readonly WarehouseDbContext _context;

    public WarehouseTransferItemRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<WarehouseTransferItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.WarehouseTransferItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public IQueryable<WarehouseTransferItem> GetQueryable()
        => _context.WarehouseTransferItems.AsQueryable();

    public IQueryable<WarehouseItem> GetWarehouseItemQueryable()
        => _context.WarehouseItems.AsQueryable();

    public async Task AddAsync(WarehouseTransferItem item, CancellationToken cancellationToken = default)
        => await _context.WarehouseTransferItems.AddAsync(item, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<WarehouseTransferItem> items, CancellationToken cancellationToken = default)
        => await _context.WarehouseTransferItems.AddRangeAsync(items, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}