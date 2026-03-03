using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Command.DeductBalance.DeductBalanceCommand;

namespace SalesService.Application.Features.Wallets.Command.DeductBalance;

public sealed record DeductBalanceCommand(Guid WalletId,Guid UserId, DeductBalanceCommandRequest Request) :
    IRequest<Result<DeductBalanceCommandResponse>>
{

    public sealed class DeductBalanceCommandRequest
    {
        public decimal Amount { get; init; }
        public byte[] RowVersion { get; init; } = null!;
        public string? Description { get; init; }
        public PaymentMethod? PaymentMethod { get; init; } 
        public string? ExternalTransactionId { get; init; }
    }

    public sealed class DeductBalanceCommandResponse
    {
        public Guid WalletId { get; init; }
        public Guid UserId { get; init; }
        public decimal BalanceBefore { get; init; }
        public decimal BalanceAfter { get; init; }
        public decimal AmountDeducted { get; init; }
        public string? TransactionCode { get; init; }
        public Guid TransactionId { get; init; } 
        public DateTimeOffset UpdatedAt { get; init; }
        public byte[] NewRowVersion { get; init; } = null!;
    }
}

