using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.ApplicationDbContext;
using IdentityService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure.Persistences.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(IAppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<User?> AddWithRoleAsync(User? user)
        {
            await _dbSet.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var createdUser = await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            return createdUser;
        }

        public async Task<IEnumerable<User?>> GetAllWithRoleAsync()
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .AsSingleQuery()
                .ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string? email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            var user = await _dbContext.Users
                .Include(u => u.Role)
                .Where(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();
            return user;
        }

        public override async Task<User?> GetByIdAsync(Guid id)
        {
            var user = await _dbContext.Users
               .Include(u => u.Role)
               .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            return user;
        }
    }
}
