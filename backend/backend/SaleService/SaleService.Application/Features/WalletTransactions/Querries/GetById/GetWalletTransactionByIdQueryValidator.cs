using FluentValidation;

namespace SalesService.Application.Features.WalletTransactions.Queries.GetById;

public sealed class GetWalletTransactionByIdQueryValidator
    : AbstractValidator<GetWalletTransactionByIdQuery>
{
    public GetWalletTransactionByIdQueryValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("TransactionId is required.");

        RuleFor(x => x.LoggedUserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
