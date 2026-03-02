using ProductService.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Commons;
using ProductService.Infrastructure.ApplicationDbContext;
using System.Linq;
using System.Linq.Expressions;

namespace ProductService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        protected readonly IProductDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(IProductDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public virtual async Task AddOnlyAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task<Guid> AddAsync(T entity)
        {
            await AddOnlyAsync(entity);
            await _dbContext.SaveChangesAsync();
            return (entity as IEntity)?.Id ?? Guid.Empty;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task DeleteOnlyAsync(Guid id)
        {
            var entityFromDb = await GetByIdAsync(id)
                ?? throw new DirectoryNotFoundException($"{typeof(T).Name} is not found");
            if (entityFromDb is SoftDeletedEntity softEntity && softEntity.IsDeleted == false)
            {
                softEntity.IsDeleted = true;
            }
            else
            {
                _dbSet.Remove(entityFromDb);
            }
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            await DeleteOnlyAsync(id);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<Guid> ids)
        {
            var entities = await GetByIdsAsync(ids.ToArray());
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
            await _dbContext.SaveChangesAsync();
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

        public virtual async Task<T?> GetByIdAsync(Guid id)
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

        public virtual async Task<T[]> GetByIdsAsync(Guid[] ids)
        {
            var query = _dbSet.AsQueryable().Where(e => ids.Contains(e.Id));
            if (typeof(SoftDeletedEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Cast<SoftDeletedEntity>().Where(x => x.IsDeleted == false).Cast<T>();
            }
            return await query.ToArrayAsync();
        }

        public virtual async void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual async Task<int> UpdateAsync(T entity)
        {
            var entityFromDb = await GetByIdAsync(entity.Id)
                ?? throw new KeyNotFoundException($"Không tìm thấy {typeof(T).Name}");

            _dbContext.Entry(entityFromDb).CurrentValues.SetValues(entity);

            if (_dbContext.Entry(entityFromDb).Metadata.FindProperty("CreatedAt") != null)
            {
                _dbContext.Entry(entityFromDb).Property("CreatedAt").IsModified = false;
            }

            return await _dbContext.SaveChangesAsync();
        }
    }
}