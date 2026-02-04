using Ardalis.Result;
using MediatR;
using static SalesService.Application.Features.Wallets.Command.DeductBalance.DeductBalanceCommand;

namespace SalesService.Application.Features.Wallets.Command.DeductBalance;

public sealed record DeductBalanceCommand(Guid WalletId, DeductBalanceCommandRequest Request) :
    IRequest<Result<DeductBalanceCommandResponse>>
{

    public sealed class DeductBalanceCommandRequest
    {
        public decimal Amount { get; init; }
        public byte[] RowVersion { get; init; } = null!;
    }

    public sealed class DeductBalanceCommandResponse
    {
        public Guid WalletId { get; init; }
        public Guid UserId { get; init; }

        public decimal BalanceBefore { get; init; }
        public decimal BalanceAfter { get; init; }
        public decimal AmountDeducted { get; init; }

        public Guid? TransactionId { get; init; }//  Link to WalletTransaction

        public DateTimeOffset UpdatedAt { get; init; }
        public byte[] NewRowVersion { get; init; } = null!; //  Để dùng cho next operations
    }
}

