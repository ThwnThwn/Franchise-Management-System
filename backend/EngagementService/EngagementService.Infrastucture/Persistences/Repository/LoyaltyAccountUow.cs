using EngagementService.Application.Persistences.Repository;
using EngagementService.Infrastucture.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EngagementService.Infrastucture.Persistences.Repository
{
    public class LoyaltyAccountUow(EngagementDbContext dbContext
        ) : ILoyaltyAccountUow
    {
        private readonly EngagementDbContext _dbContext = dbContext;

        private IDbContextTransaction? _currentTransaction; // EF Core Tx

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_currentTransaction != null) return;

            await _dbContext.Database.OpenConnectionAsync(ct);

            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(ct);
        }

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

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_currentTransaction != null) await _currentTransaction.RollbackAsync(ct);
            ClearTransaction();
        }

        private void ClearTransaction()
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
}
