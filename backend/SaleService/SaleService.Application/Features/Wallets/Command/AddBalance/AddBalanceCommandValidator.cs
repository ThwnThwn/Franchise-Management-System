using FluentValidation;
using SalesService.Application.Features.Wallets.Command.AddBalance;

namespace SalesService.Application.Features.Wallets.Validators;

public sealed class AddBalanceCommandValidator
    : AbstractValidator<AddBalanceCommand>
{
    public AddBalanceCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty()
            .WithMessage("WalletId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Request is required.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.")
                .LessThanOrEqualTo(1_000_000_000) // 1 billion VND max
                .WithMessage("Amount exceeds maximum allowed.");

            RuleFor(x => x.Request.RowVersion)
                .NotNull()
                .NotEmpty()
                .WithMessage("RowVersion is required for concurrency control.");
        });
    }
}