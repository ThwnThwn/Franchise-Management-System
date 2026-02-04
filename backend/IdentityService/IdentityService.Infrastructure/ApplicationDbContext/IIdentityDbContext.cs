using IdentityService.Domain.Commons;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IdentityService.Infrastructure.ApplicationDbContext
{
    public interface IIdentityDbContext : IDisposable
    {
        DbSet<Permission> Permissions { get; set; }

        DbSet<RefreshToken> RefreshTokens { get; set; }

        DbSet<Role> Roles { get; set; }

        DbSet<RolePermission> RolePermissions { get; set; }

        DbSet<User> Users { get; set; }

        public DbSet<T> Set<T>() where T : class, IEntity;

        public EntityEntry<T> Entry<T>(T entity) where T : class;

        public int SaveChanges();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}