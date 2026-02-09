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
                .Where(p => p.Roles.Any(r => r.Id == roleId && !r.IsDeleted)
                            && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetListBySlugsAsync(IEnumerable<string> permissionSlugs)
        {
            return await _dbContext.Permissions.AsNoTracking()
                .Where(p => permissionSlugs.Contains(p.Slug) && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetSlugsByRoleSlugAsync(string roleSlug, bool onlyActive)
        {
            var query = _dbContext.Permissions.AsNoTracking();

            query = query.Where(p => p.Roles.Any(r => r.Slug == roleSlug && !r.IsDeleted));

            if (onlyActive)
            {
                query = query.Where(p => p.IsActive);
            }

            return await query
                .Select(p => p.Slug)
                .ToListAsync();
        }
    }
}