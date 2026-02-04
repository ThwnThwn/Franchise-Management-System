using IdentityService.Application.Dtos.Permission;
using MediatR;

namespace IdentityService.Application.Features.Permission.Queries.GetAllGrouped
{
    public class GetPermissionsByIdQuery : IRequest<IEnumerable<PermissionGroupDto>>
    { }
}