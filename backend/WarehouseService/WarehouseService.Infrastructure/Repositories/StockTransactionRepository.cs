using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Infrastructure.DbContext;

namespace WarehouseService.Infrastructure.Repositories;

public class StockTransactionRepository : IStockTransactionRepository
{
    private readonly WarehouseDbContext _dbContext;

    public StockTransactionRepository(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // S1125 fix: remove == false
    public async Task<StockTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockTransactions
            .FirstOrDefaultAsync(st => st.Id == id && !st.IsDeleted, cancellationToken);
    }

    // S1125 fix: remove == false
    public async Task<bool> CodeExistsAsync(string transactionCode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StockTransactions
            .Where(st => st.TransactionCode == transactionCode && !st.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(st => st.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(StockTransaction stockTransaction, CancellationToken cancellationToken = default)
    {
        await _dbContext.StockTransactions.AddAsync(stockTransaction, cancellationToken);
    }

    public IQueryable<StockTransaction> GetQueryable()
    {
        return _dbContext.StockTransactions.AsQueryable();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // S1125 fix: remove == false
    public Task<List<StockTransaction>> GetByTransferIdAsync(Guid transferId, CancellationToken cancellationToken)
    {
        return _dbContext.StockTransactions
            .Where(st => st.Id == transferId && !st.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}