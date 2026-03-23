using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences;
using SalesService.Domain.Commons;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        protected readonly ISalesDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ISalesDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public virtual void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public virtual async Task<Guid> AddAsync(T entity, CancellationToken cancellationToken)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var entityFromDb = await GetByIdAsync(id, cancellationToken)
                ?? throw new DirectoryNotFoundException($"{typeof(T).Name} is not found");
            if (entityFromDb is SoftDeletedEntity softEntity && softEntity.IsDeleted == false)
            {
                softEntity.IsDeleted = true;
            }
            else
            {
                _dbSet.Remove(entityFromDb);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
        {
            var entities = await GetByIdsAsync(ids.ToArray(), cancellationToken);
            foreach (var entity in entities)
            {
                if (entity is SoftDeletedEntity softEntity && softEntity.IsDeleted == false)
                {
                    softEntity.IsDeleted = true;
                }
                else
                {
                    _dbSet.Remove(entity);
                }
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, object>>[]? includes = null, Expression<Func<T, bool>>? predicate = null)
        {
            var query = _dbSet.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (typeof(SoftDeletedEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Cast<SoftDeletedEntity>()
                             .Where(x => x.IsDeleted == false)
                             .Cast<T>();
            }

            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity is SoftDeletedEntity softEntity && softEntity.IsDeleted == false)
            {
                return entity;
            }
            else if (entity is SoftDeletedEntity softEntity2 && softEntity2.IsDeleted != false)
            {
                return null;
            }
            return entity;
        }

        public virtual async Task<T[]> GetByIdsAsync(Guid[] ids,
            CancellationToken cancellationToken)
        {
            var query = _dbSet.AsQueryable().Where(e => ids.Contains(e.Id));
            if (typeof(SoftDeletedEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Cast<SoftDeletedEntity>().Where(x => x.IsDeleted == false).Cast<T>();
            }
            return await query.ToArrayAsync();
        }

        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual async Task<int> UpdateAsync(T entity,
            CancellationToken cancellationToken)
        {
            var entityFromDb = await GetByIdAsync(entity.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Không tìm thấy {typeof(T).Name}");

            _dbContext.Entry(entityFromDb).CurrentValues.SetValues(entity);

            if (_dbContext.Entry(entityFromDb).Metadata.FindProperty("CreatedAt") != null)
            {
                _dbContext.Entry(entityFromDb).Property("CreatedAt").IsModified = false;
            }

            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}