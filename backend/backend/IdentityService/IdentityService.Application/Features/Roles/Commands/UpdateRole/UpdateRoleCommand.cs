using IdentityService.Application.Dtos.Role.Request;
using IdentityService.Application.Dtos.Role.Respone;
using MediatR;

namespace IdentityService.Application.Features.Roles.Commands.UpdateRole;

public record UpdateRoleCommand(Guid Id, UpdateRoleReq Request) : IRequest<RoleRep?>;
