using IdentityService.Domain.Entities;

namespace IdentityService.Application.Persistences.Repositories
{
    public interface IPermissionRepository : IGenericRepository<Permission>
    {
        Task<IEnumerable<Permission>> GetAllByRoleId(Guid roleId);

        Task<IEnumerable<Permission>> GetListBySlugsAsync(IEnumerable<string> permissionSlugs);

        Task<IEnumerable<string>> GetSlugsByRoleSlugAsync(string slug, bool isOnlyActived);
    }
}