using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.Update;

public record UpdateUserCommand(Guid Id, UpdateUserReq Request) : IRequest<UserRep?>;
