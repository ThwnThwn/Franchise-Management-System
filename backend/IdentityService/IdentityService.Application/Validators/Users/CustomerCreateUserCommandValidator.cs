using FluentValidation;
using IdentityService.Application.Features.Users.Commands.CustomerCreateUser;

namespace IdentityService.Application.Validators.Users;

public class CustomerCreateUserCommandValidator : AbstractValidator<CustomerCreateUserCommand>
{
    public CustomerCreateUserCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid");

        RuleFor(x => x.Request.RoleSlug)
            .NotEmpty().WithMessage("RoleSug is required")
            .MinimumLength(5).WithMessage("RoleSug is invalid");

        RuleFor(x => x.Request.Phone)
            .MaximumLength(20).WithMessage("Phone is too long")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Phone));

        RuleFor(x => x.Request.Address)
            .MaximumLength(255).WithMessage("Address is too long")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Address));
    }
}
