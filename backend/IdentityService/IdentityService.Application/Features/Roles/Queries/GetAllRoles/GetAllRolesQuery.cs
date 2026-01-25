using IdentityService.Application.Dtos.Role.Respone;
using MediatR;

namespace IdentityService.Application.Features.Roles.Queries.GetAllRoles;

public record GetAllRolesQuery() : IRequest<IReadOnlyList<RoleRep>>;
