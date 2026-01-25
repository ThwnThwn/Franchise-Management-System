using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.AdminCreateUser;

public record AdminCreateUserCommand(CreateUserReq Request) : IRequest<UserRep>;
