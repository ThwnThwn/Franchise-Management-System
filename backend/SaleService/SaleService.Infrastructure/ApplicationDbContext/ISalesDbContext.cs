using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SalesService.Domain.Commons;
using SalesService.Domain.Entities;

namespace SalesService.Infrastructure.ApplicationDbContext;

public interface ISalesDbContext : IDisposable
{
    DbSet<Order> Orders { get; set; }
    DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<T> Set<T>() where T : class, IEntity;

    public EntityEntry<T> Entry<T>(T entity) where T : class;

    public int SaveChanges();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}