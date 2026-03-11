using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.ApplicationDbContext;
using IdentityService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace IdentityService.Infrastructure.Persistences.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(IIdentityDbContext dbContext) : base(dbContext)
        {
        }
        /// <summary>
        /// Gets all details asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Role>> GetAllDetailsAsync()
        {
            return await _dbContext.Roles
                .Include(r => r.Permissions)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }
        /// <summary>
        /// Gets the by identifier details asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<Role?> GetByIdDetailsAsync(Guid id)
        {
            var role = await _dbContext.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == id);
            return role;
        }
        /// <summary>
        /// Gets the by name asynchronous.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public async Task<Role?> GetByNameAsync(string name)
        {
            var role = await _dbContext.Roles
                .Where(r => r.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();
            return role;
        }
        /// <summary>
        /// Gets the by slug asynchronous.
        /// </summary>
        /// <param name="slug">The slug.</param>
        /// <returns></returns>
        public async Task<Role?> GetBySlugAsync(string slug)
        {
            var role = await _dbContext.Roles
                .Where(r => r.Slug.ToLower() == slug.ToLower())
                .FirstOrDefaultAsync();
            return role;
        }
    }
}