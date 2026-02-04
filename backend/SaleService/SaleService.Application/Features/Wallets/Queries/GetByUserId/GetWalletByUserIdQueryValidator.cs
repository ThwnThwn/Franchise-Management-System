using FluentValidation;

namespace SalesService.Application.Features.Wallets.Queries.GetByUserId
{
    public sealed class GetWalletByUserIdQueryValidator:
        AbstractValidator<GetWalletByUserIdQuery>
    {
        public GetWalletByUserIdQueryValidator()
        {
            RuleFor(x => x.UserId)
           .NotEmpty().WithMessage("UserId is required.")
           .NotEqual(Guid.Empty).WithMessage("UserId must not be an empty GUID.");
        }
    }
}
