using IdentityService.Application.Dtos.Role.Respone;
using MediatR;

namespace IdentityService.Application.Features.Roles.Queries.GetPermissionsByRoleId
{
    public class GetPermissionsByRoleIdQuery : IRequest<IEnumerable<RolePermissionGroupRes>>
    {
        public Guid Id { get; set; }
    }
}