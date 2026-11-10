using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProductService.Domain.Commons;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.ApplicationDbContext;

public interface IProductDbContext : IDisposable
{
    DbSet<Category> Categories { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<ProductVariant> ProductVariants { get; set; }

    public DbSet<T> Set<T>() where T : class, IEntity;

    public EntityEntry<T> Entry<T>(T entity) where T : class;

    public int SaveChanges();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}