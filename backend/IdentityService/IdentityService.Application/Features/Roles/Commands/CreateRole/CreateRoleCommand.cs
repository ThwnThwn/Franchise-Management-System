using IdentityService.Application.Dtos.Role.Request;
using IdentityService.Application.Dtos.Role.Respone;
using MediatR;

namespace IdentityService.Application.Features.Roles.Commands.CreateRole;

public record CreateRoleCommand(CreateRoleReq Request) : IRequest<RoleRep>;
