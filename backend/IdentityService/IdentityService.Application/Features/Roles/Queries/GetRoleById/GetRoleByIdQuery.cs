using IdentityService.Application.Dtos.Role.Respone;
using MediatR;

namespace IdentityService.Application.Features.Roles.Queries.GetRoleById;

public record GetRoleByIdQuery(Guid Id) : IRequest<RoleRep?>;
