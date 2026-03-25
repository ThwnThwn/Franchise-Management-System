using EngagementService.Application.Persistences.Repository;
using EngagementService.Infrastucture.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EngagementService.Infrastucture.Persistences.Repository;

public class EngagementUow(EngagementDbContext dbContext
    ) : IEngagementUow
{
    private readonly EngagementDbContext _dbContext = dbContext;

    private IDbContextTransaction? _currentTransaction; // EF Core Tx

    /// <summary>
    /// Begins the transaction asynchronous.
    /// </summary>
    /// <param name="ct">The ct.</param>
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction != null) return;

        await _dbContext.Database.OpenConnectionAsync(ct);

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(ct);
    }

    /// <summary>
    /// Saves the changes asynchronous.
    /// </summary>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _dbContext.SaveChangesAsync(ct);

            await (_currentTransaction?.CommitAsync(ct) ?? Task.CompletedTask);

            return result;
        }
        catch
        {
            await RollbackAsync(ct);
            throw;
        }
        finally { ClearTransaction(); }
    }

    /// <summary>
    /// Rollbacks the asynchronous.
    /// </summary>
    /// <param name="ct">The ct.</param>
    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_currentTransaction != null) await _currentTransaction.RollbackAsync(ct);
        ClearTransaction();
    }

    /// <summary>
    /// Clears the transaction.
    /// </summary>
    private void ClearTransaction()
    {
        _currentTransaction?.Dispose();
        _currentTransaction = null;
    }
}