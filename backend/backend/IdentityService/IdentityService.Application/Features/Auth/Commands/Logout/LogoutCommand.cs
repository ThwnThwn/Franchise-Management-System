using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken, string? AccessToken = null) : IRequest<bool>;
