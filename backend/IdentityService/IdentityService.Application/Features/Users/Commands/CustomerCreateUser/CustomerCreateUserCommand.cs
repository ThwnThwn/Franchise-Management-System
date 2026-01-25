using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.CustomerCreateUser;

public record CustomerCreateUserCommand(CustomerCreateUserReq Request) : IRequest<UserRep>;
