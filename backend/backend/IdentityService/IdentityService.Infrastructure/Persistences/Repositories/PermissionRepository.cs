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
        /// <summary>
        /// Gets all by role identifier.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Permission>> GetAllByRoleId(Guid roleId)
        {
            return await _dbContext.Permissions.AsNoTracking()
                .Where(p => p.Roles.Any(r => r.Id == roleId && !r.IsDeleted)
                            && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        /// <summary>
        /// Gets the list by slugs asynchronous.
        /// </summary>
        /// <param name="permissionSlugs">The permission slugs.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Permission>> GetListBySlugsAsync(IEnumerable<string> permissionSlugs)
        {
            return await _dbContext.Permissions.AsNoTracking()
                .Where(p => permissionSlugs.Contains(p.Slug) && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        /// <summary>
        /// Gets the slugs by role slug asynchronous.
        /// </summary>
        /// <param name="roleSlug">The role slug.</param>
        /// <param name="onlyActive">if set to <c>true</c> [only active].</param>
        /// <returns></returns>
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