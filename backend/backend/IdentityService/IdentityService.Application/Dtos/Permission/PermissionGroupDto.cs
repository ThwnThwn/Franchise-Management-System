namespace IdentityService.Application.Dtos.Permission
{
    public class PermissionGroupDto
    {
        public required string Resource { get; set; } // Tên nhóm (User, Product...)
        public required IEnumerable<PermissionDto> Permissions { get; set; }
    }

    public class PermissionDto
    {
        public Guid Id { get; set; }
        public required string Slug { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}