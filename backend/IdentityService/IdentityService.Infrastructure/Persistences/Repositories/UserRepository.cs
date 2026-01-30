using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
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
        public UserRepository(IIdentityDbContext dbContext) : base(dbContext)
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

        public async Task<User?> GetByUsernameAsync(string? username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            var user = await _dbContext.Users
                .Include(u => u.Role)
                .Where(u => u.Username.ToLower() == username.ToLower())
                .FirstOrDefaultAsync();
            return user;
        }

        public async Task<User?> GetByEmailOrUserNameAsync(string? emailOrUserName)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserName))
                return null;

            var key = emailOrUserName.Trim();

            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Email == key || u.Username == key
                );
        }

        public override async Task<User?> GetByIdAsync(Guid id)
        {
            var user = await _dbContext.Users
               .Include(u => u.Role)
               .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            return user;
        }

        public async Task<User?> GetManagerByStoreIdAsync(Guid? storeId)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .Where(u =>
                    u.HomeStoreId == storeId &&
                    u.Role.Slug == "manager" &&
                    !u.IsDeleted
                )
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleSlugAsync(string? roleSlug)
        {
            var query = _dbContext.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(roleSlug))
            {
                query = query.Where(u => u.Role.Slug == roleSlug);
            }

            return await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<PagedResult<User>> GetPagedUsersAsync(string? keyword,Guid? id, string? roleSlug, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            var query = _dbContext.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .Where(u => !u.IsDeleted && u.Id != id);

            if (!string.IsNullOrEmpty(keyword))
            {
                var lowerKeyword = keyword.ToLower();

                query = query.Where(u =>
                    (u.Email != null && u.Email.ToLower().Contains(lowerKeyword)) ||
                    (u.Username != null && u.Username.ToLower().Contains(lowerKeyword)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(lowerKeyword)) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(lowerKeyword))
                );
            }

            if (!string.IsNullOrEmpty(roleSlug))
            {
                query = query.Where(u => u.Role.Slug == roleSlug);
            }

            query = query.OrderByDescending(u => u.CreatedAt);
            return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
        }
    }
}