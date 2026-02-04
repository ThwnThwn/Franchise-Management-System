using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories
{
    public class RefundTicketRepository : GenericRepository<RefundTicket>, IRefundTicketRepository
    {
        private readonly ISalesDbContext _context;
        public RefundTicketRepository(ISalesDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RefundTicket>> GetByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken)
        {
            return await _context.Set<RefundTicket>()
                .Where(rt => rt.Id == ticketId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RefundTicket>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            return await _context.Set<RefundTicket>()
                .Where(rt => rt.UserId == userId)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RefundTicket>> GetByStatusAsync(
            RefundTicketStatus status,
            CancellationToken cancellationToken)
        {
            return await _context.Set<RefundTicket>()
                .Where(rt => rt.Status == status)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RefundTicket>> GetByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken)
        {
            return await _context.Set<RefundTicket>()
                .Where(rt => rt.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RefundTicket>> GetByTransactionIdAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefundTicket>()
                .AsNoTracking()
                .Where(rt => rt.OriginalTransactionId == transactionId && !rt.IsDeleted)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> UpdateStatusAsync(
            Guid id,
            RefundTicketStatus status,
            CancellationToken cancellationToken)
        {
            var refundTicket = await _context.Set<RefundTicket>()
                .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);

            if (refundTicket == null)
                return 0;

            refundTicket.Status = status;
            refundTicket.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> ApproveRefundAsync(
            Guid id,
            string? approvedBy,
            string? approvalNotes,
            CancellationToken cancellationToken)
        {
            var refundTicket = await _context.Set<RefundTicket>()
                .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);

            if (refundTicket == null)
                return 0;

            refundTicket.Status = RefundTicketStatus.Approved;
            refundTicket.ApprovedBy = approvedBy;
            refundTicket.ApprovalNotes = approvalNotes;
            refundTicket.ApprovedAt = DateTime.UtcNow;
            refundTicket.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> RejectRefundAsync(
            Guid id,
            string reason,
            string? rejectedBy,
            CancellationToken cancellationToken)
        {
            var refundTicket = await _context.Set<RefundTicket>()
                .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);

            if (refundTicket == null)
                return 0;

            refundTicket.Status = RefundTicketStatus.Rejected;
            refundTicket.RejectionReason = reason;
            refundTicket.RejectedBy = rejectedBy;
            refundTicket.RejectedAt = DateTime.UtcNow;
            refundTicket.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CompleteRefundAsync(
            Guid id,
            Guid transactionId,
            DateTimeOffset processedDate,
            CancellationToken cancellationToken)
        {
            var refundTicket = await _context.Set<RefundTicket>()
                .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);

            if (refundTicket == null)
                return 0;

            refundTicket.Status = RefundTicketStatus.Completed;
            refundTicket.TransactionId = transactionId;
            refundTicket.ProcessedDate = processedDate;
            refundTicket.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalRefundAmountByRangAsync(
            DateTimeOffset fromDate,
            DateTimeOffset toDate,
            RefundTicketStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<RefundTicket>()
                .Where(rt => rt.CreatedAt >= fromDate && rt.CreatedAt <= toDate);

            if (status.HasValue)
            {
                query = query.Where(rt => rt.Status == status.Value);
            }

            return await query.SumAsync(rt => rt.RefundAmount, cancellationToken);
        }

        public async Task<Dictionary<RefundTicketStatus, int>> GetRefundCountByStatusAsync(
            DateTimeOffset fromDate,
            DateTimeOffset toDate,
            CancellationToken cancellationToken)
        {
            return await _context.Set<RefundTicket>()
                .Where(rt => rt.CreatedAt >= fromDate && rt.CreatedAt <= toDate)
                .GroupBy(rt => rt.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
        }

        public async Task<IEnumerable<RefundTicket>> GetRefundsByDateRangeAsync(
            DateTimeOffset fromDate,
            DateTimeOffset toDate,
            CancellationToken cancellationToken)
        {
            return await _context.Set<RefundTicket>()
                .Where(rt => rt.CreatedAt >= fromDate && rt.CreatedAt <= toDate)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }


        public async Task<bool> ExistingRefundWithPendingStatusAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefundTicket>()
                .AsNoTracking()
                .AnyAsync(
                    rt => rt.OriginalTransactionId == transactionId &&
                          rt.Status == RefundTicketStatus.Pending &&
                          !rt.IsDeleted,
                    cancellationToken);
        }

        
    }       
}