using System.Linq.Expressions;
using ProductService.Domain.Commons;

namespace ProductService.Application.Repositories
{
    public interface IGenericRepository<T> where T : IEntity
    {
        Task<Guid> AddAsync(T entity);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, object>>[]? includes = null,
            Expression<Func<T, bool>>? predicate = null
            );
        Task<int> UpdateAsync(T entity);
        Task<T?> GetByIdAsync(Guid id);
        Task AddRangeAsync(IEnumerable<T> entities); // to add multiple entities at once such as seed data
        Task DeleteRangeAsync(IEnumerable<Guid> ids); // to delete multiple entities at once
        void Remove(T entity);
        Task<T[]> GetByIdsAsync(Guid[] ids); // get multiple entities by their ids 
    }
}
