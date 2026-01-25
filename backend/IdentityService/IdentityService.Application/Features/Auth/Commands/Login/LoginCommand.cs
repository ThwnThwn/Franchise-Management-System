using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Dtos.Auth.Response;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Login;

public record LoginCommand(LoginReq Request) : IRequest<LoginRep>;

