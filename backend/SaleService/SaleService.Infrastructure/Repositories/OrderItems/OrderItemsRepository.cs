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
        /// <summary>
        /// Removes the order items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void RemoveOrderItems(List<OrderItem> items)
        {
            _context.OrderItems.RemoveRange(items);
        }
        /// <summary>
        /// Adds the order items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddOrderItems(List<OrderItem> items)
        {
            _context.OrderItems.AddRange(items);
        }
        /// <summary>
        /// Saves the asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
