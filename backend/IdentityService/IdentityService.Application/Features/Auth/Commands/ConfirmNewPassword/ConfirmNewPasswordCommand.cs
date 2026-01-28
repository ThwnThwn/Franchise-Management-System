using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Dtos.Auth.Response;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.ConfirmNewPassword
{
    public record class ConfirmNewPasswordCommand(ConfirmOtpReq Request) : IRequest<ConfirmOtpForgetPasswordOtpRep>;
}
