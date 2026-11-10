using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.ChangePassword
{
    public record class ChangePasswordComand(Guid Id, ChangePasswordReq Request) : IRequest<UserRep>;
}
