using IdentityService.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Persistences.Repositories
{
    public interface IGenericRepository<T> where T : IEntity
    {
        Task<Guid> AddAsync(T entity);

        Task DeleteAsync(Guid id);

        Task<IEnumerable<T>> GetAllAsync
        (
            Expression<Func<T, object>>[]? includes = null,
            Expression<Func<T, bool>>? predicate = null
        );

        Task<int> UpdateAsync(T entity);

        Task<T?> GetByIdAsync(Guid id);

        Task AddRangeAsync(IEnumerable<T> entities);

        Task DeleteRangeAsync(IEnumerable<Guid> ids);

        void Remove(T entity);

        Task<T[]> GetByIdsAsync(Guid[] ids);
    }
}