using MediatR;
using static SalesService.Application.Features.Orders.Commands.PayWithWallet.PayOrderWithWalletCommand;
using Ardalis.Result;
namespace SalesService.Application.Features.Orders.Commands.PayWithWallet;

public sealed record PayOrderWithWalletCommand(
    Guid OrderId,
    Guid CustomerId
) : IRequest<Result<PayOrderWithWalletResponse>>
{
    public sealed class PayOrderWithWalletResponse
    {
        public Guid OrderId { get; init; }
        public string OrderCode { get; init; } = string.Empty;
        public Guid CustomerWalletId { get; init; }
        public Guid ShopWalletId { get; init; }
        public decimal Amount { get; init; }
        public decimal CustomerBalanceBefore { get; init; }
        public decimal CustomerBalanceAfter { get; init; }
        public decimal ShopBalanceBefore { get; init; }
        public decimal ShopBalanceAfter { get; init; }
        public Guid CustomerTransactionId { get; init; }
        public Guid ShopTransactionId { get; init; }
        public string CustomerTransactionCode { get; init; } = string.Empty;
        public string ShopTransactionCode { get; init; } = string.Empty;
        public DateTimeOffset PaidAt { get; init; }
    }
}