using SalesService.Application.Persisitences.OrderItems;
using SalesService.Domain.Entities;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories.OrderItems
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly ISalesDbContext _context;

        public OrderItemRepository(ISalesDbContext context)
        {
            _context = context;
        }

        public void RemoveOrderItems(List<OrderItem> items)
        {
            _context.OrderItems.RemoveRange(items);
        }

        public void AddOrderItems(List<OrderItem> items)
        {
            _context.OrderItems.AddRange(items);
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
