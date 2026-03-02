using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductService.Application.Repositories;
using ProductService.Infrastructure.ApplicationDbContext;

namespace ProductService.Infrastructure.Repositories;

public class ProductUow(ProductDbContext dbContext, ICapPublisher capPublisher) : IProductUow
{
    private readonly ProductDbContext _dbContext = dbContext;
    private readonly ICapPublisher _capPublisher = capPublisher;

    private IDbContextTransaction? _currentTransaction; // EF Core Tx
    private IIntegrationTransaction? _integrationTransaction; // Wrapper Tx

    public IIntegrationTransaction? IntegrationTransaction => _integrationTransaction;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction != null) return;

        await _dbContext.Database.OpenConnectionAsync(ct);

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(_capPublisher, autoCommit: false, ct);

        if (_capPublisher.Transaction != null)
        {
            _integrationTransaction = new CapTransactionWrapper(_capPublisher.Transaction);
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _dbContext.SaveChangesAsync(ct);

            if (_integrationTransaction != null)
            {
                await _integrationTransaction.CommitAsync(ct);
            }
            else
            {
                await (_currentTransaction?.CommitAsync(ct) ?? Task.CompletedTask);
            }

            return result;
        }
        catch
        {
            await RollbackAsync(ct);
            throw;
        }
        finally { ClearTransaction(); }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_integrationTransaction != null) await _integrationTransaction.RollbackAsync(ct);
        else if (_currentTransaction != null) await _currentTransaction.RollbackAsync(ct);
        ClearTransaction();
    }

    private void ClearTransaction()
    {
        _integrationTransaction?.Dispose();
        _integrationTransaction = null;
        _currentTransaction?.Dispose();
        _currentTransaction = null;
    }
}