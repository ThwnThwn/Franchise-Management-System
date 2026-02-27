using FluentValidation;

namespace SalesService.Application.Features.Orders.Commands.PayWithWallet;

public sealed class PayOrderWithWalletValidator : AbstractValidator<PayOrderWithWalletCommand>
{
    public PayOrderWithWalletValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required");
    }
}