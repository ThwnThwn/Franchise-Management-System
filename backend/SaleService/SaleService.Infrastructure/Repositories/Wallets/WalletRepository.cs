using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        private readonly ISalesDbContext _context;

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

        // =========================================================
        // UNIT OF WORK HELPERS (Logic lõi - Đầy đủ Check & Exception)
        // =========================================================

        public void ApplyAddBalance(Wallet wallet, decimal amount, byte[] rowVersion)
        {
            if (amount <= 0)
                throw new ArgumentException("Số tiền cộng vào phải lớn hơn 0.", nameof(amount));

            // Optimistic concurrency check
            if (wallet.RowVersion == null || !wallet.RowVersion.SequenceEqual(rowVersion))
            {
                throw new DbUpdateConcurrencyException(
                    "Phiên bản ví không khớp hoặc dữ liệu đã bị thay đổi bởi giao dịch khác. Vui lòng thử lại.");
            }

            wallet.Balance += amount;
        }

        public void ApplyDeductBalance(Wallet wallet, decimal amount, byte[] rowVersion)
        {
            if (amount <= 0)
                throw new ArgumentException("Số tiền trừ đi phải lớn hơn 0.", nameof(amount));

            // Kiểm tra số dư khả dụng
            if (wallet.Balance < amount)
            {
                throw new InvalidOperationException(
                    $"Số dư không đủ. Hiện có: {wallet.Balance}, Cần rút: {amount}");
            }

            // Kiểm tra phiên bản dữ liệu
            if (wallet.RowVersion == null || !wallet.RowVersion.SequenceEqual(rowVersion))
            {
                throw new DbUpdateConcurrencyException(
                    "Phiên bản ví đã cũ. Ví đã được cập nhật bởi một tiến trình khác, vui lòng thử lại.");
            }

            wallet.Balance -= amount;
        }

        public async Task<bool> AddBalanceAsync(
        Guid walletId,
        decimal amount,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
        {
            try
            {
                var wallet = await _dbSet.FirstOrDefaultAsync(
                    w => w.Id == walletId && !w.IsDeleted, cancellationToken);

                if (wallet == null) return false;

                // Gọi hàm Apply để xử lý logic và check concurrency
                ApplyAddBalance(wallet, amount, rowVersion);

                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException) { throw; }
            catch (ArgumentException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add balance to wallet {walletId}", ex);
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
                var wallet = await _dbSet.FirstOrDefaultAsync(
                    w => w.Id == walletId && !w.IsDeleted, cancellationToken);

                if (wallet == null) return false;

                // Gọi hàm Apply để xử lý logic, check balance và concurrency
                ApplyDeductBalance(wallet, amount, rowVersion);

                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (ArgumentException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deduct balance from wallet {walletId}", ex);
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

        public async Task<Wallet?> GetForUpdateAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(
                    w => w.Id == walletId && !w.IsDeleted,
                    cancellationToken);
        }

        public void ApplyUpdateBalance(
            Wallet wallet,
            decimal newBalance,
            byte[] rowVersion)
        {
            // 1. Kiểm tra Version ngay lập tức
            if (wallet.RowVersion == null || !wallet.RowVersion.SequenceEqual(rowVersion))
            {
                throw new DbUpdateConcurrencyException(
                    "Dữ liệu ví đã bị thay đổi bởi giao dịch khác. Vui lòng tải lại.");
            }

            // 2. Cập nhật trạng thái trong Tracking
            wallet.Balance = newBalance;
            wallet.UpdatedAt = DateTimeOffset.UtcNow;

            // Đánh dấu thực thể là đã bị sửa đổi
            _dbSet.Update(wallet);
        }

        public async Task<bool> UpdateBalanceAsync(
            Guid walletId,
            decimal newBalance,
            byte[] rowVersion,
            CancellationToken cancellationToken = default)
        {
            var wallet = await _dbSet.FirstOrDefaultAsync(w => w.Id == walletId && !w.IsDeleted, cancellationToken);
            if (wallet == null) return false;

            ApplyUpdateBalance(wallet, newBalance, rowVersion);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}