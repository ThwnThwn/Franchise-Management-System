using System.Threading;
using System.Threading.Tasks;
using SalesService.Application.Interfaces.Orders;
using SalesService.Domain.Entities;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories.Orders;

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

    async Task<Order> IOrderRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _context.Orders.Find(id);
        await _context.SaveChangesAsync(cancellationToken);
        throw new NotImplementedException();
    }
}