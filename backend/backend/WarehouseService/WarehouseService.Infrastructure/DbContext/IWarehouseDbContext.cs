using Microsoft.EntityFrameworkCore;
using WarehouseService.Domain.Entities;

namespace WarehouseService.Infrastructure.DbContext;

public interface IWarehouseDbContext : IDisposable
{
    DbSet<Warehouse> Warehouses { get; set; }
    DbSet<WarehouseItem> WarehouseItems { get; set; }
    DbSet<StockTransaction> StockTransactions { get; set; }
    DbSet<WarehouseTransfer> WarehouseTransfers { get; set; }
    DbSet<WarehouseTransferItem> WarehouseTransferItems { get; set; }


    public int SaveChanges();
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}