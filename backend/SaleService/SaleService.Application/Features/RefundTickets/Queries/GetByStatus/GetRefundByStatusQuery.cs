using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Queries.GetByStatus.GetRefundByStatusQuery;

namespace SalesService.Application.Features.RefundTickets.Queries.GetByStatus;

public sealed record GetRefundByStatusQuery(
    Guid? UserId, 
    RefundTicketStatus Status,
    int PageIndex = 1,
    int PageSize = 10
) : IRequest<Result<BuildingBlocks.Core.Pagination.PagedResult<RefundTicketResponse>>>
{
    public sealed class RefundTicketResponse
    {
        public Guid Id { get; set; }
        public string TicketCode { get; set; } = null!;
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid OriginalTransactionId { get; set; }

        // Amounts
        public decimal Amount { get; set; }
        public decimal RefundAmount { get; set; }

        // Status & Reason
        public RefundTicketStatus Status { get; set; }
        public string? Reason { get; set; }
        public RefundReasonType? RefundReasonType { get; set; }

        // Timestamps
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // ✅ OPTION 1: Keep nullable for cleaner API response
        public Guid? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public string? ApprovalNotes { get; set; }

        public Guid? RejectedBy { get; set; }
        public DateTimeOffset? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }
        public Guid? TransactionId { get; set; }

        public Guid? CancelledBy { get; set; }
        public DateTimeOffset? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
    }
}