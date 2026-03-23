using Microsoft.EntityFrameworkCore.Storage;

namespace SalesService.Application.Persisitences;

public interface IUnitOfWork : IDisposable
{
    // Lưu các thay đổi vào DB (SaveChanges thông thường)
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Quản lý Transaction thủ công cho các case nghiệp vụ phức tạp
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}