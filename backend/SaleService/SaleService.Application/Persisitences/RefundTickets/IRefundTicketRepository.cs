using SalesService.Domain.Entities;
using BuildingBlocks.Core.Pagination;
using SalesService.Domain.Enums;

namespace SalesService.Application.Persisitences
{
    public interface IRefundTicketRepository : IGenericRepository<RefundTicket>
    {
        Task<RefundTicket> GetByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken);

        Task<PagedResult<RefundTicket>> GetByUserIdAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken);

        Task<IEnumerable<RefundTicket>> GetByStatusAsync(
            RefundTicketStatus status,
            CancellationToken cancellationToken);

       
        Task<IEnumerable<RefundTicket>> GetByTransactionIdAsync(
        Guid OriginalTransactionId,
        CancellationToken cancellationToken);

        Task<PagedResult<RefundTicket>> GetAllRefundsAsync(
        Guid? userId,
        RefundTicketStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

        Task<PagedResult<RefundTicket>> GetByStatusAsync(
            RefundTicketStatus status,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<PagedResult<RefundTicket>> GetByUserIdAndStatusAsync(
            Guid userId,
            RefundTicketStatus status,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);

        // Status updates
        Task<int> UpdateStatusAsync(
            Guid id,
            RefundTicketStatus status,
            CancellationToken cancellationToken);

        Task<int> ApproveRefundAsync(
            Guid id,
            Guid? approvedBy,
            string? approvalNotes,
            CancellationToken cancellationToken);

        Task<int> RejectRefundAsync(
            Guid id,
            string reason,
            Guid? rejectedBy,
            CancellationToken cancellationToken);

        Task<int> CompleteRefundAsync(
            Guid id,
            Guid transactionId,
            DateTimeOffset processedDate,
            CancellationToken cancellationToken);

        // Statistics & Reporting
        Task<decimal> GetTotalRefundAmountByRangAsync(
            DateTimeOffset fromDate,
            DateTimeOffset toDate,
            RefundTicketStatus? status = null,
            CancellationToken cancellationToken = default);

        Task<Dictionary<RefundTicketStatus, int>> GetRefundCountByStatusAsync(
            DateTimeOffset fromDate,
            DateTimeOffset toDate,
            CancellationToken cancellationToken);

        Task<IEnumerable<RefundTicket>> GetRefundsByDateRangeAsync(
            DateTimeOffset fromDate,
            DateTimeOffset toDate,
            CancellationToken cancellationToken);


        Task<bool> ExistingRefundWithPendingStatusAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default);

        Task<decimal> GetTotalRefundedAmountByTransactionIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default);


    }
}