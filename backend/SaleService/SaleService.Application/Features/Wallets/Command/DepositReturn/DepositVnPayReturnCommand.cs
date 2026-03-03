using Ardalis.Result;
using MediatR;

namespace SalesService.Application.Features.Wallets.Deposit;

public sealed record DepositVnPayReturnCommand(
    IDictionary<string, string> QueryParams
) : IRequest<Result<DepositVnPayReturnRes>>;

public sealed class DepositVnPayReturnRes
{
    public bool Success { get; set; }
    public Guid WalletTransactionId { get; set; }
    public decimal Amount { get; set; }
    public decimal NewBalance { get; set; } 
}