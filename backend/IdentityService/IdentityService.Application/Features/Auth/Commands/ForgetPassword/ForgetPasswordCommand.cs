using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Dtos.Auth.Response;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.ForgetPassword
{
    public record class ForgetPasswordCommand(ForgetPasswordReq Request) : IRequest<Unit>;
}
