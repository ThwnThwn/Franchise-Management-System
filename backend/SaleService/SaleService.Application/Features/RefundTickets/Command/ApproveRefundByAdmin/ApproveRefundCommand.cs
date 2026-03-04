using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.ApproveRefund.ApproveRefundCommand;

namespace SalesService.Application.Features.RefundTickets.Command.ApproveRefund;

public sealed record ApproveRefundCommand(
    Guid RefundTicketId,
    ApproveRefundRequest Request
) : IRequest<Result<ApproveRefundResponse>>
{
    public sealed class ApproveRefundRequest
    {
        public string? ApprovalNotes { get; set; }
        public bool AutoComplete { get; set; } = false;
    }

    public sealed class ApproveRefundResponse
    {
        public Guid RefundTicketId { get; set; }
        public string TicketCode { get; set; } = null!;
        public RefundTicketStatus Status { get; set; }
        public decimal Amount { get; set; } 
        public Guid CustomerId { get; set; } 
        public DateTimeOffset ApprovedAt { get; set; }
        public string? ApprovalNotes { get; set; }
        public string NextSteps { get; set; } = null!; 
    }
}