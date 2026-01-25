using FluentValidation;
using IdentityService.Application.Features.Roles.Commands.UpdateRole;

namespace IdentityService.Application.Validators.Roles;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Role Id is required");

        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(50);
    }
}
