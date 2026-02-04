using MediatR;

namespace IdentityService.Application.Features.Permission.Commands.UpdateInfo
{
    public class UpdatePermissionInfoCommand : IRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}