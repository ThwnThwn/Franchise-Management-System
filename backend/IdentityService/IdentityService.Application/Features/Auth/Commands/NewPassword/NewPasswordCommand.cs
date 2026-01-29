using IdentityService.Application.Dtos.Auth.Request;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Features.Auth.Commands.NewPassword
{
    public record class NewPasswordCommand(string Email ,NewPasswordReq Request) : IRequest<Unit>;
}
