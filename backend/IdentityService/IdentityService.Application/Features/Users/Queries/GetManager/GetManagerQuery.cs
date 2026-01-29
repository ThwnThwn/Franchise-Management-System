using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetManager
{
    public record GetManagerQuery(GetManagerReq Request) : IRequest<UserRep>;
}
