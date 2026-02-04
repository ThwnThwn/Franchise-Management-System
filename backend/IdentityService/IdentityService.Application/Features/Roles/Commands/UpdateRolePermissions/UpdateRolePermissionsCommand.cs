using MediatR;

namespace IdentityService.Application.Features.Roles.Commands.UpdateRolePermissions
{
    public class UpdateRolePermissionsCommand : IRequest
    {
        public Guid Id { get; set; }
        public IEnumerable<string> PermissionsToGrant { get; set; } = [];
        public IEnumerable<string> PermissionsToRevoke { get; set; } = [];
    }
}