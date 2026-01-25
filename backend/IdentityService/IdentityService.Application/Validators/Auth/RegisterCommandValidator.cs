using FluentValidation;
using IdentityService.Application.Features.Auth.Commands.Register;

namespace IdentityService.Application.Validators.Auth;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(5).WithMessage("Password must be at least 5 characters");

        RuleFor(x => x.Request.Phone)
            .NotEmpty().WithMessage("Phone is required");

        RuleFor(x => x.Request.Address)
            .NotEmpty().WithMessage("Address is required");
    }
}
