using FluentValidation;

namespace SalesService.Application.Features.Wallets.Command.Create
{
    public sealed class CreateWalletCommandValidator
        : AbstractValidator<CreateWalletCommand>
    {
        public CreateWalletCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId must not be empty");
        }
    }
}
