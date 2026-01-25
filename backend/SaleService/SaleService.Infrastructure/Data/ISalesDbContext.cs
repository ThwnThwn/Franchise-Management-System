using Microsoft.EntityFrameworkCore;
using SaleService.Domain.Entities;

namespace SaleService.Infrastructure.Data;

public interface ISalesDbContext
{
    DbSet<Order> Orders { get; set; }
    DbSet<OrderItem> OrderItems { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
