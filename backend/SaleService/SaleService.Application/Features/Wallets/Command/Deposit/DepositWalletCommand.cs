using Ardalis.Result;
using MediatR;

namespace SalesService.Application.Features.Wallets.Deposit;

public sealed record DepositWalletCommand(
    Guid UserId,
    DepositWalletReq Request
) : IRequest<Result<DepositWalletRes>>;

public sealed class DepositWalletReq
{
    public decimal Amount { get; set; }
    public string? ReturnUrl { get; set; }
}

public sealed class DepositWalletRes
{
    public string PaymentUrl { get; set; } = default!; 
}