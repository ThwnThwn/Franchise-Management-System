using System.Threading.Tasks;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Persisitences
{
    public interface IRefundTicketRepository : IGenericRepository<RefundTicket>
    {
        Task<IEnumerable<RefundTicket>> GetByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken);

        Task<IEnumerable<RefundTicket>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken);

        Task<IEnumerable<RefundTicket>> GetByStatusAsync(
            RefundTicketStatus status,
            CancellationToken cancellationToken);

        Task<IEnumerable<RefundTicket>> GetByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken);

        Task<IEnumerable<RefundTicket>> GetByTransactionIdAsync(
        Guid OriginalTransactionId,
        CancellationToken cancellationToken);

        // Status updates
        Task<int> UpdateStatusAsync(
            Guid id,
            RefundTicketStatus status,
            CancellationToken cancellationToken);

        Task<int> ApproveRefundAsync(
            Guid id,
            string? approvedBy,
            string? approvalNotes,
            CancellationToken cancellationToken);

        Task<int> RejectRefundAsync(
            Guid id,
            string reason,
            string? rejectedBy,
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
    }
}