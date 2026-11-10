using FluentValidation;
using IdentityService.Application.Features.Users.Commands.UpdateUser;

namespace IdentityService.Application.Validators.Users;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User Id is required");

        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Email)
            //.NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid");

        RuleFor(x => x.Request.Phone)
            .MaximumLength(20).WithMessage("Phone is too long")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Phone));

        RuleFor(x => x.Request.Address)
            .MaximumLength(255).WithMessage("Address is too long")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Address));

        RuleFor(x => x.Request.Password)
            .MinimumLength(5).WithMessage("Password must be at least 5 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Password));
    }
}