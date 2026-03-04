using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.SetCompleteRefund.SetCompleteRefundCommand;
namespace SalesService.Application.Features.RefundTickets.Command.SetCompleteRefund;

public sealed record SetCompleteRefundCommand(SetCompleteRefundRequest request) : IRequest<Result<SetCompleteRefundResponse>>
{
    public sealed class SetCompleteRefundRequest
    {
        public Guid RefundTicketId { get; init; }
        public RefundTicketStatus Status { get; init; }
        public string? CompleteNotes { get; set; }
    }
    public sealed class SetCompleteRefundResponse
    {
        public Guid RefundTicketId { get; init; }
        public RefundTicketStatus Status { get; init; }
        public string? CompleteNotes { get; set; }
    }
}
