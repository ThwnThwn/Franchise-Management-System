using MediatR;

namespace IdentityService.Application.Features.Roles.Queries.GetPermissionsBySlug
{
    public class GetPermissionsBySlugQuery : IRequest<IEnumerable<string>>
    {
        public string RoleSlug { get; set; } = string.Empty;
    }
}