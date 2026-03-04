using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

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

            // Sử dụng EF.Functions.Like hoặc so sánh trực tiếp nếu DB đã set case-insensitive
            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == email.ToLower());
            //.FirstOrDefaultAsync(u => u.Email == email);
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

        public async Task<IEnumerable<User?>> GetByIds(IReadOnlyList<Guid> ids)
        {
            return await _dbContext.Users
                .Where(u => ids.Contains(u.Id))
                .Cast<User?>()
                .ToListAsync();
        }
        public async Task<PagedResult<User>> GetPagedUsersAsync(string? keyword, List<Guid>? ids, Guid? StoredId, bool? HasHourlyWage, string? roleSlug, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            var query = _dbContext.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .Where(u => !u.IsDeleted);

            // Nếu mà không null thì tức đang có filter loyalty 
            if (ids != null && ids.Any())
            {
                query = query.Where(u => ids.Contains(u.Id));
            }

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
            // Filter by role
            if (!string.IsNullOrEmpty(roleSlug))
            {
                query = query.Where(u => u.Role.Slug == roleSlug);
            }
            // Filter by StoreId
            if (!string.IsNullOrEmpty(StoredId.ToString()))
            {
                query = query.Where(u => u.HomeStoreId == StoredId);
            }
            // Filter by bool CurrentHourlyWage
            if (HasHourlyWage == true)
            {
                query = query.Where(u => u.CurrentHourlyWage != null);
            }
            query = query.OrderByDescending(u => u.CreatedAt);
            return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
        }

        public async Task<User?> GetByPhoneAsync(string? phone)
        {
            // 1. Kiểm tra đầu vào sớm để tránh truy vấn vô ích
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            // 2. Thực hiện truy vấn an toàn
            var customer = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Phone != null && u.Phone == phone);
            return customer;
        }

        public async Task<List<User>> GetUsersByPhoneAsync(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                phone = "";

            var customers = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Phone != null && u.Phone.Contains(phone))
                .ToListAsync();
            return customers;
        }

        public async Task<List<User>> GetUsersByEmailAsync(string? email)
        {
            // 2. Thực hiện truy vấn an toàn
            if (string.IsNullOrWhiteSpace(email))
                email = "";

            var customers = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Email != null && u.Email.ToLower().Contains(email.ToLower()))
                .ToListAsync();
            return customers;
        }
    }
}