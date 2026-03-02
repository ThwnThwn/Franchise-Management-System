using DotNetCore.CAP;
using ProductService.Application.Repositories;

namespace ProductService.Infrastructure.Repositories;

public class CapTransactionWrapper(ICapTransaction capTransaction) : IIntegrationTransaction
{
    public ICapTransaction InternalTransaction { get; } = capTransaction;

    public Task CommitAsync(CancellationToken ct = default) => InternalTransaction.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default) => InternalTransaction.RollbackAsync(ct);

    public void Dispose()
    {
        InternalTransaction.Dispose();
        GC.SuppressFinalize(this);
    }
}