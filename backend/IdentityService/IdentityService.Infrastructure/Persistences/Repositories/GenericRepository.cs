using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Commons;
using IdentityService.Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure.Persistences.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        protected readonly IIdentityDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(IIdentityDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public virtual async Task<Guid> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
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
            var entityFromDb = await GetByIdAsync(entity.Id)
                ?? throw new KeyNotFoundException($"Không tìm thấy {typeof(T).Name}");

            _dbContext.Entry(entityFromDb).CurrentValues.SetValues(entity);

            if (_dbContext.Entry(entityFromDb).Metadata.FindProperty("CreatedAt") != null)
            {
                _dbContext.Entry(entityFromDb).Property("CreatedAt").IsModified = false;
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

        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any()) return;

            // 1. Lấy danh sách ID từ input
            var entityIds = entities.Select(e => e.Id).ToList();

            // 2. QUERY 1 LẦN DUY NHẤT: Lấy tất cả entity trong DB trùng ID
            // Lưu ý: Cần DbSet<T> để query, giả sử _dbSet là DbSet<T>
            var dbEntities = await _dbSet
                .Where(e => entityIds.Contains(e.Id))
                .ToListAsync();

            // 3. Validate: Kiểm tra xem có thiếu thằng nào không
            if (dbEntities.Count != entityIds.Count)
            {
                // Tìm những ID không tồn tại để báo lỗi chi tiết
                var foundIds = dbEntities.Select(e => e.Id);
                var missingIds = entityIds.Except(foundIds);
                throw new KeyNotFoundException($"Không tìm thấy các Id: {string.Join(", ", missingIds)}");
            }

            // 4. Xử lý Mapping trên RAM (Nhanh siêu tốc)
            // Chuyển dbEntities sang Dictionary để tìm kiếm O(1) thay vì quét list
            var dbEntityDict = dbEntities.ToDictionary(e => e.Id);

            foreach (var entity in entities)
            {
                if (dbEntityDict.TryGetValue(entity.Id, out var entityFromDb))
                {
                    // Copy giá trị
                    _dbContext.Entry(entityFromDb).CurrentValues.SetValues(entity);

                    // Bảo vệ CreatedAt
                    var createdAtProp = _dbContext.Entry(entityFromDb).Metadata.FindProperty("CreatedAt");
                    if (createdAtProp != null)
                    {
                        _dbContext.Entry(entityFromDb).Property("CreatedAt").IsModified = false;
                    }
                }
            }

            // 5. Commit 1 lần
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
}