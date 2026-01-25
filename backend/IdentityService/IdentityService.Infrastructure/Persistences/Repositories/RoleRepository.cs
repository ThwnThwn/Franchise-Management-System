using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.ApplicationDbContext;
using IdentityService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistences.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(IAppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Role>> GetAllDetailsAsync()
        {
            return await _dbContext.Roles
                .ToListAsync();
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            var role = await _dbContext.Roles
                .Where(r => r.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();
            return role;
        }
    }
}