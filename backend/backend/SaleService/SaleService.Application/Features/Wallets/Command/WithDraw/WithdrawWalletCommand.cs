using Ardalis.Result;
using MediatR;

namespace SalesService.Application.Features.Wallets.Withdraw;

public sealed record WithdrawWalletCommand(
    Guid UserId,
    WithdrawWalletReq Request
) : IRequest<Result<WithdrawWalletRes>>;

public sealed class WithdrawWalletReq
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
}

public sealed class WithdrawWalletRes
{
    public Guid WithdrawTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = default!;
    public string? Message { get; set; }
}