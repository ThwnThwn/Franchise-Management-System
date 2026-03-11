using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using BuildingBlocks.Core.Pagination;

namespace SalesService.Application.Persisitences.WalletTransactions;

public interface IWalletTransactionRepository : IGenericRepository<WalletTransaction>
{
    Task<PagedResult<WalletTransaction>> GetAllTransactionsAsync(
    int page, int pageSize,
    CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletTransaction>> GetAllByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<WalletTransaction>> GetByWalletIdPaginatedAsync(
        Guid walletId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<WalletTransaction>> GetByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletTransaction>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<WalletTransaction?> GetByOrderIdSingleAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<WalletTransaction?> GetByReferenceAsync(
        Guid referenceId,
        WalletReferenceType referenceType,
        CancellationToken cancellationToken = default);

    Task<WalletTransaction?> GetByTransactionCodeAsync(
        string transactionCode,
        CancellationToken cancellationToken = default);

    Task<WalletTransaction?> GetByExternalTransactionIdAsync(
       string externalTransactionId,
       CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletTransaction>> GetByStatusAsync(
        TransactionStatus status,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletTransaction>> GetByTypeAsync(
        TransactionType type,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletTransaction>> GetByWalletAndStatusAsync(
        Guid walletId,
        TransactionStatus status,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletTransaction>> GetPendingByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletTransaction>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletTransaction>> GetByWalletAndDateRangeAsync(
        Guid walletId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByTransactionCodeAsync(
        string transactionCode,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingTransactionsAsync(
        Guid walletId,
        CancellationToken cancellationToken = default);

    Task<bool> IsOrderPaidAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(
        Guid transactionId,
        TransactionStatus newStatus,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteTransactionAsync(
        Guid transactionId,
        string? completionNotes,
        CancellationToken cancellationToken = default);

    Task<bool> FailTransactionAsync(
        Guid transactionId,
        string failureReason,
        CancellationToken cancellationToken = default);

    Task<decimal> GetTotalAmountAsync(
        Guid walletId,
        TransactionType? type,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<Dictionary<TransactionStatus, int>> GetCountByStatusAsync(
        Guid? walletId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletTransaction>> GetRecentTransactionsAsync(
        Guid walletId,
        int count = 10,
        CancellationToken cancellationToken = default);

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