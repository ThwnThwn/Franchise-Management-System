using IdentityService.Domain.Entities;

namespace IdentityService.Application.Persistences.Repositories
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name);

        Task<Role?> GetBySlugAsync(string slug);

        Task<IEnumerable<Role>> GetAllDetailsAsync();
        Task<Role?> GetByIdDetailsAsync(Guid id);
    }
}