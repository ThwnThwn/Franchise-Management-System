using FluentValidation;

namespace SalesService.Application.Features.WalletTransactions.Queries.GetByCode;

public sealed class GetWalletTransactionByCodeQueryValidator
    : AbstractValidator<GetWalletTransactionByCodeQuery>
{
    public GetWalletTransactionByCodeQueryValidator()
    {
        RuleFor(x => x.TransactionCode)
            .NotEmpty()
            .WithMessage("TransactionCode is required.")
            .MaximumLength(100)
            .WithMessage("TransactionCode is too long.");
    }
}
