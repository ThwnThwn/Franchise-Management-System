using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin.RejectRefundCommand;
namespace SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin;

public sealed record RejectRefundCommand(
Guid InternalUserId,
Guid RefundTicketId,
RejectRefundRequest Request
) : IRequest<Result<RejectRefundResponse>>
{
    public sealed class RejectRefundRequest
    {
        public string? RejectNotes { get; set; }
    }
    public sealed class RejectRefundResponse
    {
        public Guid RefundTicketId { get; set; }
        public string TicketCode { get; set; } = null!;
        public RefundTicketStatus Status { get; set; }
        public decimal Amount { get; set; }
        public Guid CustomerId { get; set; }
        public Guid RejectdBy { get; set; }
        public DateTimeOffset RejectdAt { get; set; }
        public string? RejectNotes { get; set; }
        public string NextSteps { get; set; } = null!;
    }
}