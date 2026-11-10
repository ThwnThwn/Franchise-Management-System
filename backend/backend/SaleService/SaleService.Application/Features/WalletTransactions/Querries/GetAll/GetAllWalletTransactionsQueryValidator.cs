using FluentValidation;

using SalesService.Application.Features.WalletTransactions.Queries.GetAll;

namespace SalesService.Application.Features.WalletTransactions.Querries.GetAll
{
    public sealed class GetAllWalletTransactionsQueryValidator :
        AbstractValidator<GetAllWalletTransactionsQuery>
    {
        public GetAllWalletTransactionsQueryValidator()
        {
            RuleFor(x => x.LoggedUserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

        }
    }
}
