using FluentValidation;

namespace SalesService.Application.Features.Wallets.Command.AddBalance;

public sealed class AddBalanceCommandValidator
    : AbstractValidator<AddBalanceCommand>
{
    public AddBalanceCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty()
            .WithMessage("Wallet ID is required");

        RuleFor(x => x.Request.Amount)
            .GreaterThan(0)
            .WithMessage("Amount value must be greater than 0");

        RuleFor(x => x.Request.RowVersion)
            .NotNull()
            .WithMessage("RowVersion is required for concurrency control.")
            .NotEmpty()
            .WithMessage("RowVersion cannot be empty. Please reload wallet data and retry.");
    }
}
