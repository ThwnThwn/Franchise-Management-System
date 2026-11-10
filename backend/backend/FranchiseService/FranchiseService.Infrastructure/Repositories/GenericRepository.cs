using System.Linq.Expressions;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Commons;
using FranchiseService.Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;

namespace FranchiseService.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
{
    protected readonly IFranchiseDbContext _dbContext;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(IFranchiseDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
    }

    /// <summary>
    /// Adds the only asynchronous.
    /// </summary>
    /// <param name="entity">The entity.</param>
    public virtual async Task AddOnlyAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// Adds the asynchronous.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns></returns>
    public virtual async Task<Guid> AddAsync(T entity)
    {
        await AddOnlyAsync(entity);
        await _dbContext.SaveChangesAsync();
        return (entity as IEntity)?.Id ?? Guid.Empty;
    }

    public virtual async Task DeleteAsync(Guid id)
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
        await _dbContext.SaveChangesAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, object>>[]? includes = null,
        Expression<Func<T, bool>>? predicate = null
    )
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
        //return  await _dbSet.FirstOrDefault(t => t.Id == id && t.);
        var entityFromDb = await _dbSet.FindAsync(id);
        if (entityFromDb is SoftDeletedEntity softEntity1 && softEntity1.IsDeleted == false)
        {
            return entityFromDb;
        }
        else if (entityFromDb is SoftDeletedEntity softEntity2 && softEntity2.IsDeleted != false)
        {
            return null;
        }
        return entityFromDb;
    }

    public virtual async Task<T[]> GetByIdsAsync(Guid[] ids)
    {
        var query = _dbSet.AsQueryable().Where(e => ids.Contains(e.Id));
        if (typeof(SoftDeletedEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Cast<SoftDeletedEntity>()
                         .Where(x => x.IsDeleted == false)
                         .Cast<T>();
        }
        return await query.ToArrayAsync();
    }

    public virtual async Task<int> UpdateAsync(T entity)
    {
        var trackedEntity = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
        if (trackedEntity == null)
        {
            trackedEntity = await GetByIdAsync(entity.Id)
                ?? throw new KeyNotFoundException();
        }

        _dbContext.Entry(trackedEntity).CurrentValues.SetValues(entity);

        if (_dbContext.Entry(trackedEntity).Metadata.FindProperty("CreatedAt") != null)
        {
            _dbContext.Entry(trackedEntity).Property("CreatedAt").IsModified = false;
        }

        return await _dbContext.SaveChangesAsync();
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null || !entities.Any())
            return;

        await _dbSet.AddRangeAsync(entities);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<Guid> ids)
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
}