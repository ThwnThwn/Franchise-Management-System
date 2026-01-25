using IdentityService.Application.Dtos.Auth.Response;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.TokenRefresh;

public record TokenRefreshCommand(string RefreshToken) : IRequest<RefreshTokenRep>;
