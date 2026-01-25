using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.AdminCreate;

public record AdminCreateUserCommand(CreateUserReq Request) : IRequest<UserRep>;
