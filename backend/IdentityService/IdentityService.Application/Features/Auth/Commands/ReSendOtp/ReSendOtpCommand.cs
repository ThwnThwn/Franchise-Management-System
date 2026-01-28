using IdentityService.Application.Dtos.Auth.Request;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Features.Auth.Commands.ReSendOtp
{
    public record class ReSendOtpCommand(ReSendOtpReq Request) : IRequest<Unit>;
}
