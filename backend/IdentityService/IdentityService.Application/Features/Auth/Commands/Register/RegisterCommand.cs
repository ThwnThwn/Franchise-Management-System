using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Dtos.Auth.Response;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Register
{
    public record RegisterCommand(RegisterReq Request) : IRequest<RegisterRep>{}
}
