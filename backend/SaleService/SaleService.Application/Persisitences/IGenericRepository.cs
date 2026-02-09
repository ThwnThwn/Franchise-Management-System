using System.Linq.Expressions;
using SalesService.Domain.Commons;

namespace SalesService.Application.Persisitences
{
    public interface IGenericRepository<T> where T : IEntity
    {
        Task<Guid> AddAsync(T entity,
            CancellationToken cancellationToken);
        Task DeleteAsync(Guid id,
            CancellationToken cancellationToken);
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, object>>[]? includes = null,
            Expression<Func<T, bool>>? predicate = null
            );
        Task<int> UpdateAsync(T entity,
            CancellationToken cancellationToken);
        Task<T?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken);
        Task AddRangeAsync(IEnumerable<T> entities,
            CancellationToken cancellationToken); // to add multiple entities at once such as seed data
        Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken); // to delete multiple entities at once
        void Remove(T entity);
        Task<T[]> GetByIdsAsync(Guid[] ids, CancellationToken cancellationToken); // get multiple entities by their ids 
    }
}
