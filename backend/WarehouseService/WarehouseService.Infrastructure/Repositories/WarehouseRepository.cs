using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Infrastructure.DbContext;

namespace WarehouseService.Infrastructure.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly WarehouseDbContext _dbContext;

    public WarehouseRepository(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // S1125 fix: remove == false
    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);
    }

    // S1125 fix: remove == false
    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Warehouses
            .Where(w => w.Code == code && !w.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(w => w.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        await _dbContext.Warehouses.AddAsync(warehouse, cancellationToken);
    }

    public IQueryable<Warehouse> GetQueryable()
    {
        return _dbContext.Warehouses.AsQueryable();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // S1125 fix: remove == false
    public Task<bool> ExistWithStoreId(Guid storeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Warehouses
            .AnyAsync(w => w.StoreId == storeId && !w.IsDeleted, cancellationToken);
    }

    // S1125 fix: remove == false
    public Task<bool> ExistWithStoreAddress(string address, CancellationToken cancellationToken = default)
    {
        return _dbContext.Warehouses
            .AnyAsync(w => w.Address.Equals(address) && !w.IsDeleted, cancellationToken);
    }
}