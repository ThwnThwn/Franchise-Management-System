using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.RefundTickets.Queries.GetById;

public sealed record GetRefundByIdQuery(Guid RefundTicketId,Guid UserId)
    : IRequest<Result<GetRefundByIdResponse>>;
public sealed class GetRefundByIdResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TicketCode { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public Guid WalletId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid OriginalTransactionId { get; set; }
    public RefundTicketStatus Status { get; set; }
    public RefundReasonType RefundReason { get; set; }
    public RefundTicketMethod RefundTicketMethod { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}