using BuildingBlocks.Core.Pagination;
using IdentityService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Persistences.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string? email);
        Task<IEnumerable<User?>> GetByIds(IReadOnlyList<Guid> ids);
        Task<User?> GetByUsernameAsync(string?  username);
        Task<User?> GetByEmailOrUserNameAsync(string? emailOrUserName);
        Task<IEnumerable<User?>> GetAllWithRoleAsync();
        Task<User?> GetByPhoneAsync(string? phone);
        Task<User?> AddWithRoleAsync(User? user);
        Task<User?> GetManagerByStoreIdAsync(Guid? storeId);
        Task<IEnumerable<User?>> GetUsersByRoleSlugAsync(string? roleSlug);
        Task<PagedResult<User>> GetPagedUsersAsync(string? keyword,Guid? id, string? roleSlug, int pageIndex, int pageSize, CancellationToken cancellationToken);

        Task<List<User>> GetUsersByPhoneAsync(string? phone);
        Task<List<User>> GetUsersByEmailAsync(string? email);
    }
}
