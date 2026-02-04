
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.ApplicationDbContext;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
namespace SalesService.Infrastructure.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        private readonly  ISalesDbContext _context;
        public WalletRepository(ISalesDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<Wallet?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.UserId == userId && !w.IsDeleted,
                    cancellationToken);
        }

        public async Task<bool> ExistsByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(
                    w => w.UserId == userId && !w.IsDeleted,
                    cancellationToken);
        }


        public async Task<bool> AddBalanceAsync(
            Guid walletId,
            decimal amount,
            byte[] rowVersion,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (amount <= 0)
                {
                    throw new ArgumentException("Amount must be greater than 0", nameof(amount));
                }

                var wallet = await _dbSet
                    .FirstOrDefaultAsync(
                        w => w.Id == walletId && !w.IsDeleted,
                        cancellationToken);

                if (wallet == null)
                    return false;

                // kiểm tra xem amount có bị cập nhật nào trước khi thực hiện feature gì đó về amount
                if (!wallet.RowVersion.SequenceEqual(rowVersion))
                {
                    throw new DbUpdateConcurrencyException(
                        "Wallet was modified by another transaction. Please retry.");
                }

                wallet.Balance += amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to add balance to wallet {walletId}", ex);
            }
        }

        public async Task<bool> DeductBalanceAsync(
            Guid walletId,
            decimal amount,
            byte[] rowVersion,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (amount <= 0)
                {
                    throw new ArgumentException("Amount must be greater than 0", nameof(amount));
                }

                // Get wallet
                var wallet = await _dbSet
                    .FirstOrDefaultAsync(
                        w => w.Id == walletId && !w.IsDeleted,
                        cancellationToken);

                if (wallet == null)
                    return false;

                // Check  balance đủ không
                if (wallet.Balance < amount)
                {
                    throw new InvalidOperationException(
                        $"Insufficient balance. Current: {wallet.Balance}, Required: {amount}");
                }

                // Optimistic concurrency check
                if (!wallet.RowVersion.SequenceEqual(rowVersion))
                {
                    throw new DbUpdateConcurrencyException(
                        "Wallet was modified by another transaction. Please retry.");
                }

                // Deduct balance
                wallet.Balance -= amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _context.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to deduct balance from wallet {walletId}", ex);
            }
        }


        public async Task<bool> UpdateStatusAsync(
            Guid walletId,
            WalletStatus status,
            CancellationToken cancellationToken = default)
        {
            var wallet = await _dbSet
                .FirstOrDefaultAsync(
                    w => w.Id == walletId && !w.IsDeleted,
                    cancellationToken);

            if (wallet == null)
                return false;

            if (!IsValidStatusTransition(wallet.Status, status))
            {
                throw new InvalidOperationException(
                    $"Invalid status transition from {wallet.Status} to {status}");
            }

            wallet.Status = status;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }


        public async Task<bool> HasSufficientBalanceAsync(
            Guid walletId,
            decimal amount,
            CancellationToken cancellationToken = default)
        {
            var balance = await _dbSet
                .Where(w => w.Id == walletId && !w.IsDeleted)
                .Select(w => w.Balance)
                .FirstOrDefaultAsync(cancellationToken);

            return balance >= amount;
        }

        public async Task<bool> HasPendingIssuesAsync(
            Guid walletId,
            CancellationToken cancellationToken = default)
        {
            // Check for pending wallet transactions
            var hasPendingTransactions = await _context.WalletTransactions
                .AnyAsync(
                    t => t.Id == walletId &&
                         t.Status == TransactionStatus.Pending &&
                         !t.IsDeleted,
                    cancellationToken);

            if (hasPendingTransactions)
                return true;

            // Check for pending refund tickets
            var hasPendingRefunds = await _context.RefundTickets
                .AnyAsync(
                    r => r.Id == walletId &&
                         r.Status == RefundTicketStatus.Pending &&
                         !r.IsDeleted,
                    cancellationToken);

            return hasPendingRefunds;
        }


        public async Task<PagedResult<Wallet>> GetByStatusAsync(
            WalletStatus status,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(w => w.Status == status && !w.IsDeleted)
                .OrderByDescending(w => w.CreatedAt);

            return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
        }

        public async Task<PagedResult<Wallet>> GetAllWalletsWithPagingAsync(
            string? keyword,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(w => !w.IsDeleted);

            // Apply keyword filter if provided
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var searchTerm = keyword.Trim().ToLower();

                query = query.Where(w =>
                    w.Id.ToString().Contains(searchTerm) ||
                    w.UserId.ToString().Contains(searchTerm) ||
                    w.Status.ToString().ToLower().Contains(searchTerm)
                );
            }

            // Order by created date descending
            query = query.OrderByDescending(w => w.CreatedAt);

            // Default pagination if not provided
            int pageIndex = 1;
            int pageSize = 10;

            return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
        }

        private bool IsValidStatusTransition(WalletStatus currentStatus, WalletStatus newStatus)
        {
            // Same status is always valid
            if (currentStatus == newStatus)
                return true;

            return currentStatus switch
            {
                WalletStatus.Inactive => newStatus == WalletStatus.Active,

                WalletStatus.Active => newStatus == WalletStatus.Locked ||
                                       newStatus == WalletStatus.Suspended,

                WalletStatus.Locked => newStatus == WalletStatus.Active ||
                                       newStatus == WalletStatus.Suspended,

                // From Suspended
                WalletStatus.Suspended => newStatus == WalletStatus.Active,

                _ => false
            };
        }
    }
}