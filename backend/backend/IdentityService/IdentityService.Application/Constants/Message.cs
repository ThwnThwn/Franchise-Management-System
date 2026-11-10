using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Constants
{
    public class Message
    {
        public static class UserMessage
        {
            public const string NotFound = "user.not_found";
            public const string InvalidAccessToken = "user.invalid_access_token";
            public const string DoNotHavePermission = "user.do_not_have_permission";
            public const string ExistingUser = "user.exist_already";
        }

        public static class SystemMessage
        {
            public const string InternalServerError = "system.internal_server_error";
            public const string RolePermissionCacheMissing = "system.role_permission_cache_missing";
        }

        public static class RolePermissionMessage
        {
            public const string NotFound = "role_permission.not_found";
        }

        public static class PermissionMessage
        {
            public const string NotFound = "permission.not_found";
            public const string PermissionCanNotBeBlank = "permission.can_not_blank";
        }
        public static class RoleMessage
        {
            public const string NotFound = "role.not_found";
        }
    }
}
