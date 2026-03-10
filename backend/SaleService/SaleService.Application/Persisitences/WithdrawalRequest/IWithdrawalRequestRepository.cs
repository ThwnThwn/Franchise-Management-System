using BuildingBlocks.Core.Pagination;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Persisitences.WithdrawalRequest;

public interface IWithdrawalRequestRepository : IGenericRepository<Domain.Entities.WithdrawalRequest>
{
    // =========================================================
    // QUERIES
    // =========================================================

    Task<PagedResult<Domain.Entities.WithdrawalRequest>> GetPagedWithdrawalsAsync(
        Guid? userId,
        WithdrawalStatus? status,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Domain.Entities.WithdrawalRequest?> GetByRequestCodeAsync(
        string requestCode,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingWithdrawalAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    // =========================================================
    // COMMANDS (Thực thi và SaveChanges ngay)
    // =========================================================

    Task<int> ApproveAsync(Guid id, Guid approvedBy, string? approvalNotes, CancellationToken cancellationToken = default);

    Task<int> RejectAsync(Guid id, Guid rejectedBy, string rejectionReason, CancellationToken cancellationToken = default);

    Task<int> CancelAsync(Guid id, Guid cancelledBy, string cancellationReason, CancellationToken cancellationToken = default);

    Task<int> CompleteAsync(Guid id, Guid processedBy, Guid transactionId,
        string? receiptImageUrl, string? receiptImagePublicId, string? bankTransactionId,
        CancellationToken cancellationToken = default
    );

    // =========================================================
    // UNIT OF WORK HELPERS (Chỉ thay đổi State, dùng chung Transaction)
    // =========================================================

    void ApplyApprove(Domain.Entities.WithdrawalRequest request, Guid approvedBy, string? approvalNotes);

    void ApplyReject(Domain.Entities.WithdrawalRequest request, Guid rejectedBy, string rejectionReason);

    void ApplyCancel(Domain.Entities.WithdrawalRequest request, Guid cancelledBy, string cancellationReason);

    void ApplyComplete(
        Domain.Entities.WithdrawalRequest request,
        Guid processedBy, Guid transactionId, string? bankTransactionId,
        string? receiptImageUrl, string? receiptImagePublicId);
}