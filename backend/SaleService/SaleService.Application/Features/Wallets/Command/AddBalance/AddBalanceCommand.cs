using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Command.AddBalance.AddBalanceCommand;

namespace SalesService.Application.Features.Wallets.Command.AddBalance;

public sealed record AddBalanceCommand(
    Guid WalletId,
    Guid UserId, 
    AddBalanceCommandRequest Request
) : IRequest<Result<AddBalanceCommandResponse>>
{
    public sealed class AddBalanceCommandRequest
    {
        public decimal Amount { get; init; }
        public byte[] RowVersion { get; init; } = null!;
        public string? Description { get; init; }
        public PaymentMethod? PaymentMethod { get; init; } 
        public string? ExternalTransactionId { get; init; }
    }

    public sealed class AddBalanceCommandResponse
    {
        public Guid WalletId { get; init; }
        public Guid UserId { get; init; }
        public decimal BalanceBefore { get; init; }
        public decimal BalanceAfter { get; init; }
        public decimal AmountAdded { get; init; }
        public Guid TransactionId { get; init; } 
        public string?TransactionCode { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
        public byte[] NewRowVersion { get; init; } = null!;
    }
}