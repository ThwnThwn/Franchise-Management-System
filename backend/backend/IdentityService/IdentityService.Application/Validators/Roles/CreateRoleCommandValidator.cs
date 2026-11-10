using FluentValidation;
using IdentityService.Application.Features.Roles.Commands.CreateRole;

namespace IdentityService.Application.Validators.Roles;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(50);
    }
}
