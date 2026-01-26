using FluentValidation;
using IdentityService.Application.Features.Auth.Commands.Login;

namespace IdentityService.Application.Validators.Auth;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.EmailOrUsername)
            .NotEmpty().WithMessage("Email or Username is required");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(5).WithMessage("Password must be at least 5 characters");
    }
}
