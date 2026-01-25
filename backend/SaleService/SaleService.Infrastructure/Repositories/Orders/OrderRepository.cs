using System.Threading;
using System.Threading.Tasks;
using SaleService.Application.Interfaces.Orders;
using SaleService.Domain.Entities;
using SaleService.Infrastructure.Data;

namespace SaleService.Infrastructure.Repositories.Orders;

public class OrderRepository : IOrderRepository
{
    private readonly ISalesDbContext _context;

    public OrderRepository(ISalesDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }
}
