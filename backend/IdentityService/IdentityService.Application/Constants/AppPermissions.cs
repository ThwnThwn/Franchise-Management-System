using System.Reflection;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Constants
{
    public static class AppPermissions
    {
        // --- ĐỊNH NGHĨA CÁC NHÓM QUYỀN (CONST) ---
        public static class User
        {
            public const string Read = "user.read";
            public const string Create = "user.create";
            public const string Update = "user.update";
            public const string Delete = "user.delete";
        }

        public static class Role
        {
            public const string Read = "role.read";
            public const string Create = "role.create";
            public const string Update = "role.update";
            public const string Delete = "role.delete";
            public const string Toggle = "role.toggle_permission";
        }

        // --- TỰ ĐỘNG QUÉT (REFLECTION) ---
        // Biến này sẽ được dùng cho SyncService
        public static readonly IReadOnlyList<Permission> AllPermissions = GetAllPermissions();

        private static List<Permission> GetAllPermissions()
        {
            var permissions = new List<Permission>();

            // Quét các nested class (User, Role...)
            foreach (var type in typeof(AppPermissions).GetNestedTypes())
            {
                var groupName = type.Name; // Ví dụ: "User"

                // Quét các field const string
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                {
                    if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                    {
                        var slug = (string)field.GetValue(null)!;
                        var name = field.Name; // Ví dụ: "Read"

                        permissions.Add(new Permission
                        {
                            Slug = slug, // "user.read"
                            Resource = groupName, // "User"
                            Name = $"{name} {groupName}", // "Read User"
                            Description = $"Allow {name.ToLower()} {groupName.ToLower()}" // "Allow read user"
                        });
                    }
                }
            }
            return permissions;
        }
    }
}