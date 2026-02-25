using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer.CustomerCreateRefundTicketCommand;

namespace SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer;

public sealed record CustomerCreateRefundTicketCommand(
    Guid CustomerId,
    Guid OrderId,
    CustomerCreateRefundTicketRequest Request
) : IRequest<Result<CustomerCreateRefundTicketResponse>>
{
    public sealed class CustomerCreateRefundTicketRequest
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; } = null!;
    }

    public sealed class CustomerCreateRefundTicketResponse
    {
        public Guid Id { get; set; }
        public string TicketCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = null!;
        public Guid OrderId { get; set; }
        public RefundTicketStatus Status { get; set; }  // PENDING_APPROVAL
        public DateTimeOffset CreatedAt { get; set; }
    }
}
