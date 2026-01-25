using FluentValidation;
using IdentityService.Application.Features.Auth.Commands.TokenRefresh;

namespace IdentityService.Application.Validators.Auth;

public class TokenRefreshCommandValidator : AbstractValidator<TokenRefreshCommand>
{
    public TokenRefreshCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
