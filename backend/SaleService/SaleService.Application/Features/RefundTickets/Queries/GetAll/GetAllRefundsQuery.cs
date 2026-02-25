using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.RefundTickets.Queries.GetAll;

public sealed record GetAllRefundsQuery(Guid InternalUserId,
    int PageIndex = 1,
    int PageSize = 20
) : IRequest<Result<BuildingBlocks.Core.Pagination.PagedResult<GetAllRefundsResponse>>>;

public sealed class GetAllRefundsResponse
{
    public Guid Id { get; set; }
    public string TicketCode { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal RefundAmount { get; set; }

    public string? Reason { get; set; }
    public RefundReasonType? RefundReason { get; set; }

    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }

    public Guid? OrderId { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid OriginalTransactionId { get; set; }

    public RefundTicketStatus Status { get; set; }
    public RefundTicketMethod? RefundTicketMethod { get; set; }

    public Guid? ProcessedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public Guid? RejectedBy { get; set; }
    public Guid? CreatedByInternalUserId { get; set; }

    public string? ApprovalNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? InternalNotes { get; set; }
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
