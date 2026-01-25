using FluentValidation;
using IdentityService.Application.Features.Roles.Commands.DeleteRole;

namespace IdentityService.Application.Validators.Roles
{
    public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
    {
        public DeleteRoleCommandValidator() 
        {
            RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Role Id is required");
        }
    }
}
