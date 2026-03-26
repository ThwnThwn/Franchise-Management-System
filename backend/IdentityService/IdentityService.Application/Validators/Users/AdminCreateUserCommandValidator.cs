using FluentValidation;
using IdentityService.Application.Features.Users.Commands.AdminCreateUser;

namespace IdentityService.Application.Validators.Users;

public class AdminCreateUserCommandValidator : AbstractValidator<AdminCreateUserCommand>
{
    public AdminCreateUserCommandValidator()
    {
        RuleFor(x => x.Request)
        .Cascade(CascadeMode.Stop) 
        .NotNull().WithMessage("Request is required");

        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid");

        // If admin can set roleId, validate it
        RuleFor(x => x.Request.RoleSlug)
            .NotEmpty().WithMessage("RoleSlug is required");
    }
}
