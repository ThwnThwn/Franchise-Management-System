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
        Task<IEnumerable<User?>> GetAllWithRoleAsync();
        Task<User?> AddWithRoleAsync(User? user);
    }
}
