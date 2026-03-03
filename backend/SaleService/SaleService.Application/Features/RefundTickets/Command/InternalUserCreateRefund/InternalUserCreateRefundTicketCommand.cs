using Ardalis.Result;
using MediatR;
using SalesService.Application.Common.Models;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund.InternalUserCreateRefundTicketCommand;

namespace SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund;

public sealed record InternalUserCreateRefundTicketCommand(
    Guid UserId,
    InternalUserCreateRefundTicketRequest Request,
    InternalUserOverrideInfo? InternalUserOverride = null
) : IRequest<Result<InternalUserCreateRefundTicketResponse>>
{
    // ---------- REQUEST ----------
    public sealed class InternalUserCreateRefundTicketRequest
    {
        public Guid WalletId { get; set; }
        public Guid OriginalTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }

    // ---------- RESPONSE ----------
    public sealed class InternalUserCreateRefundTicketResponse
    {
        public Guid Id { get; set; }

        public string TicketCode { get; set; } = null!;

        public decimal Amount { get; set; }

        public string? Reason { get; set; }

        public Guid WalletId { get; set; }

        public Guid? TransactionId { get; set; }

        public Guid OriginalTransactionId { get; set; }

        public RefundTicketStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
