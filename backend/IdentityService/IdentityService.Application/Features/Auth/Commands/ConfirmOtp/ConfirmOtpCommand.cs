using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Dtos.Auth.Response;
using MediatR;


namespace IdentityService.Application.Features.Auth.Commands.ConfirmOtp
{
    public record class ConfirmOtpCommand(ConfirmOtpReq Request) : IRequest<Unit>;
}
