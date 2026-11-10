using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction.CreateTransactionCommand;

namespace SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction;

public sealed record CreateTransactionCommand(
    Guid UserId,
    CreateTransactionRequest Request,
    HasAdminOverride? AdminOverride = null
) : IRequest<Result<CreateTransactionResponse>>
{
    public sealed class CreateTransactionRequest
    {
        public Guid WalletId { get; set; }

        public decimal Amount { get; set; }

        public TransactionType TransactionType { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public Guid? ReferenceId { get; set; }

        public WalletReferenceType? ReferenceType { get; set; }

        public string? Description { get; set; }

        public string? Notes { get; set; }

        public string? ExternalTransactionId { get; set; }

        public Guid? DestinationWalletId { get; set; }

        public string? Metadata { get; set; }
    }

    public sealed class HasAdminOverride
    {
        public Guid AdminId { get; set; }

        public bool AutoComplete { get; set; } = false;

        public bool BypassBalanceCheck { get; set; } = false;

        public bool BypassTransactionLimits { get; set; } = false;

        public string? InternalNotes { get; set; }
    }

    public sealed class CreateTransactionResponse
    {
        public Guid Id { get; set; }
        public string TransactionCode { get; set; } = null!;
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus Status { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public Guid? ReferenceId { get; set; }
        public WalletReferenceType? ReferenceType { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}