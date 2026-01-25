using FluentValidation;
using IdentityService.Application.Features.Users.Commands.AdminCreateUser;

namespace IdentityService.Application.Validators.Users;

public class AdminCreateUserCommandValidator : AbstractValidator<AdminCreateUserCommand>
{
    public AdminCreateUserCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(5).WithMessage("Password must be at least 5 characters");

        // If admin can set roleId, validate it
        RuleFor(x => x.Request.RoleId)
            .NotEmpty().WithMessage("RoleId is required");
    }
}
