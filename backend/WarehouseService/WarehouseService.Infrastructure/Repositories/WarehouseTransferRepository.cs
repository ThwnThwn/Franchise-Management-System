using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Infrastructure.DbContext;

namespace WarehouseService.Infrastructure.Repositories;

public class WarehouseTransferRepository : IWarehouseTransferRepository
{
    private readonly WarehouseDbContext _dbContext;

    public WarehouseTransferRepository(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // S1125 fix: remove == false
    public async Task<WarehouseTransfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WarehouseTransfers
            .FirstOrDefaultAsync(wt => wt.Id == id && !wt.IsDeleted, cancellationToken);
    }

    // S1125 fix: remove == false
    public async Task<bool> CodeExistsAsync(string transferCode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.WarehouseTransfers
            .Where(wt => wt.TransferCode == transferCode && !wt.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(wt => wt.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(WarehouseTransfer warehouseTransfer, CancellationToken cancellationToken = default)
    {
        await _dbContext.WarehouseTransfers.AddAsync(warehouseTransfer, cancellationToken);
    }

    public IQueryable<WarehouseTransfer> GetQueryable()
    {
        return _dbContext.WarehouseTransfers.AsQueryable();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}