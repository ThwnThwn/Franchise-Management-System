using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Storage;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.ApplicationDbContext;

namespace ProductService.Infrastructure.Repositories;

public class ProductUow : IProductUow
{
    private readonly ProductDbContext _dbContext;
    private readonly ICapPublisher _capPublisher;
    private IDbContextTransaction? _currentTransaction;

    public ProductUow(ProductDbContext dbContext, ICapPublisher capPublisher)
    {
        _dbContext = dbContext;
        _capPublisher = capPublisher;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return;

        // Nó sẽ tự động tạo bảng Outbox và lưu Event vào đó khi DbContext SaveChanges
        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(_capPublisher, autoCommit: false, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _dbContext.SaveChangesAsync(cancellationToken);
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            return result;
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }
}