using FluentValidation;
using IdentityService.Application.Features.Users.Commands.DeleteUser;

namespace IdentityService.Application.Validators.Users;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User Id is required");
    }
}
