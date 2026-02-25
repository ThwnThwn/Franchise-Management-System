using FluentValidation;
using SalesService.Application.Features.WalletTransactions.Queries.GetByUserId;

namespace SalesService.Application.Features.WalletTransactions.Querries.GetByUserId
{
    internal class GetWalletTransactionsByUserIdQueryValidator :
        AbstractValidator<GetWalletTransactionsByUserIdQuery>
    {
        public GetWalletTransactionsByUserIdQueryValidator()
        {

            RuleFor(x => x.UserIdToSearch)
                .NotEmpty()
                .WithMessage("UserId is required to search.");
        }
    }
}
