
using BuildingBlocks.Core.Pagination;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
namespace SalesService.Application.Persisitences.Wallets;

public interface IWalletRepository : IGenericRepository<Wallet>
{
   
    Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cộng tiền vào ví với Optimistic Concurrency Control
    /// </summary>
    Task<bool> AddBalanceAsync(
        Guid walletId,
        decimal amount,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Trừ tiền khỏi ví với Optimistic Concurrency Control
    /// </summary>
    Task<bool> DeductBalanceAsync(
        Guid walletId,
        decimal amount,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);


    // ========== STATUS MANAGEMENT ==========

    /// <summary>
    /// Update trạng thái ví
    /// </summary>
    Task<bool> UpdateStatusAsync(
        Guid walletId,
        WalletStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra ví có đủ balance không
    /// </summary>
    Task<bool> HasSufficientBalanceAsync(
        Guid walletId,
        decimal amount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra ví có pending issues không (để unlock)
    /// </summary>
    Task<bool> HasPendingIssuesAsync(Guid walletId, CancellationToken cancellationToken = default);

    // ========== QUERIES & FILTERING ==========

    /// <summary>
    /// Lấy danh sách ví theo status (phân trang)
    /// </summary>
    Task<PagedResult<Wallet>> GetByStatusAsync(
        WalletStatus status,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm kiếm ví nâng cao (phân trang)
    /// </summary>
    Task<PagedResult<Wallet>> GetAllWalletsWithPagingAsync(
        string? keyword,
        CancellationToken cancellationToken = default);


    Task<Wallet?> GetForUpdateAsync(
        Guid walletId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateBalanceAsync(
        Guid walletId,
        decimal newBalance,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);
}

