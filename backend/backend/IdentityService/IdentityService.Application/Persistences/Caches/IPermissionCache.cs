using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Persistences.Caches
{
    public interface IPermissionCache
    {
        Task ClearAllRolesCacheAsync();

        Task ClearRolePermissionsCacheAsync(string roleSlug);

        Task<bool> IsRoleLoadedAsync(string roleSlug);

        Task SetRolePermissionsAsync(string roleSlug, IEnumerable<string> permissions);
    }
}