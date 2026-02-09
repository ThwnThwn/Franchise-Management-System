using Ardalis.Result;
using MediatR;
using static SalesService.Application.Features.RefundTickets.Command.CreateRefund.CreateRefundTicketCommand;

namespace SalesService.Application.Features.RefundTickets.Command.CreateRefund;

public sealed record CreateRefundTicketCommand(Guid UserId, CreateRefundTicketRequest Request) :
    IRequest<Result<CreateRefundTicketResponse>>
{
    public sealed class CreateRefundTicketRequest  {
        public Guid WalletId { get; set; }
        public Guid OriginalTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
    }

    public sealed class CreateRefundTicketResponse
    {
        public Guid Id { get; set; }
        public string TicketCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public Guid WalletId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? TransactionId { get; set; }
        public Guid OriginalTransactionId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
