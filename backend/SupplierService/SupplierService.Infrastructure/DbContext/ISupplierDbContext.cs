using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SupplierService.Domain.Commons;
using SupplierService.Domain.Entities;

namespace SupplierService.Infrastructure.DbContext;

public interface ISupplierDbContext : IDisposable
{
    DbSet<Supplier> Suppliers { get; set; }
    DbSet<SupplierIngredient> SupplierIngredients { get; set; }
    DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    public int SaveChanges();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
