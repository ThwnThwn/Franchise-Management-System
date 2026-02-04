using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistences.Repositories
{
    public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
    {
        public PermissionRepository(IIdentityDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Permission>> GetAllByRoleId(Guid roleId)
        {
            return await _dbContext.Permissions.AsNoTracking()
                .Where(p => p.RolePermissions.Any(rp => rp.RoleId == roleId && !rp.IsDeleted)
                    && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetListBySlugsAsync(IEnumerable<string> permissionSlugs)
        {
            return await _dbContext.Permissions
                .AsNoTracking()
                .Where(p => permissionSlugs.Contains(p.Slug) && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetSlugsByRoleSlugAsync(string slug, bool isOnlyActived)
        {
            var query = _dbContext.Permissions.AsNoTracking();

            if (isOnlyActived)
            {
                query = query.Where(p => p.RolePermissions.Any(rp => rp.Role.Slug == slug
                        && rp.IsActived
                        && !rp.IsDeleted)
                    && !p.IsDeleted);
            }
            else
            {
                query = query.Where(p => p.RolePermissions.Any(rp => rp.Role.Slug == slug
                        && !rp.IsDeleted)
                    && !p.IsDeleted);
            }

            return await query
                .Select(p => p.Slug)
                .ToListAsync();
        }
    }
}