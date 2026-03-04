using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.CancelRefund.CancelRefundCommand;

namespace SalesService.Application.Features.RefundTickets.Command.CancelRefund;

public sealed record CancelRefundCommand(
    Guid RefundTicketId,
    CancelRefundRequest? Request = null 
) : IRequest<Result<CancelRefundResponse>>
{
    public sealed class CancelRefundRequest
    {
        public string? CancellationReason { get; set; }
    }

    public sealed class CancelRefundResponse
    {
        public Guid RefundTicketId { get; set; }
        public string TicketCode { get; set; } = null!;
        public RefundTicketStatus Status { get; set; } // Cancelled
        public decimal Amount { get; set; }
        public string? CancellationReason { get; set; }
        public DateTimeOffset CancelledAt { get; set; }
        public string Message { get; set; } = null!; 
    }
}