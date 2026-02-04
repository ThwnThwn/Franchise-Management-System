using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using BuildingBlocks.Core.Pagination;

namespace SalesService.Application.Persisitences.WalletTransactions
{
    public interface IWalletTransactionRepository : IGenericRepository<WalletTransaction>
    {
        
        Task<IEnumerable<WalletTransaction>> GetAllByWalletIdAsync(
            Guid walletId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated transactions for a wallet
        /// </summary>
        Task<PagedResult<WalletTransaction>> GetByWalletIdPaginatedAsync(
            Guid walletId,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all transactions for a specific user
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get transaction(s) associated with an order, an order can be paid many times since not enough amount
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a single transaction by order (if only one payment per order)
        /// </summary>
        Task<WalletTransaction?> GetByOrderIdSingleAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get transaction by transaction code/reference number
        /// </summary>
        Task<WalletTransaction?> GetByTransactionCodeAsync(
            string transactionCode,
            CancellationToken cancellationToken = default);

        // =======================
        // QUERY BY STATUS & TYPE
        // =======================

        /// <summary>
        /// Get transactions by status
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetByStatusAsync(
            TransactionStatus status,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get transactions by type (Deposit, Withdrawal, Payment, Refund, etc.)
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetByTypeAsync(
            TransactionType type,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get transactions by wallet and status
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetByWalletAndStatusAsync(
            Guid walletId,
            TransactionStatus status,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get pending transactions for a wallet
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetPendingByWalletIdAsync(
            Guid walletId,
            CancellationToken cancellationToken = default);

        // =======================
        // QUERY BY DATE RANGE
        // =======================

        /// <summary>
        /// Get transactions within a date range
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetByDateRangeAsync(
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get wallet transactions within a date range
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetByWalletAndDateRangeAsync(
            Guid walletId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);

        // =======================
        // BUSINESS VALIDATIONS
        // =======================

        /// <summary>
        /// Check if a transaction code already exists
        /// </summary>
        Task<bool> ExistsByTransactionCodeAsync(
            string transactionCode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if wallet has any pending transactions
        /// </summary>
        Task<bool> HasPendingTransactionsAsync(
            Guid walletId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if order has been paid (has successful payment transaction)
        /// </summary>
        Task<bool> IsOrderPaidAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        // =======================
        // STATUS UPDATES
        // =======================

        /// <summary>
        /// Update transaction status
        /// </summary>
        Task<bool> UpdateStatusAsync(
            Guid transactionId,
            TransactionStatus newStatus,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mark transaction as completed
        /// </summary>
        Task<bool> CompleteTransactionAsync(
            Guid transactionId,
            string? completionNotes,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mark transaction as failed
        /// </summary>
        Task<bool> FailTransactionAsync(
            Guid transactionId,
            string failureReason,
            CancellationToken cancellationToken = default);

        // =======================
        // STATISTICS & REPORTING
        // =======================

        /// <summary>
        /// Get total transaction amount by wallet and type within date range
        /// </summary>
        Task<decimal> GetTotalAmountAsync(
            Guid walletId,
            TransactionType? type,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get transaction count by status
        /// </summary>
        Task<Dictionary<TransactionStatus, int>> GetCountByStatusAsync(
            Guid? walletId,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get recent transactions for a wallet
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetRecentTransactionsAsync(
            Guid walletId,
            int count = 10,
            CancellationToken cancellationToken = default);

        // =======================
        // ADVANCED FILTERING
        // =======================

        /// <summary>
        /// Get transactions with advanced filtering and pagination
        /// </summary>
        Task<PagedResult<WalletTransaction>> GetWithFilterAsync(
            Guid? walletId,
            Guid? userId,
            TransactionType? type,
            TransactionStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            string? searchKeyword,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}