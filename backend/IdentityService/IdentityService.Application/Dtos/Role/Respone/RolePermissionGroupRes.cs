namespace IdentityService.Application.Dtos.Role.Respone
{
    public class RolePermissionGroupRes
    {
        public required string Resource { get; set; }
        public required IEnumerable<RolePermissionRes> Permissions { get; set; }
    }

    public class RolePermissionRes
    {
        public Guid Id { get; set; }
        public required string Slug { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        //public required bool IsActived { get; set; }
    }
}