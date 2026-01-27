using SalesService.Domain.Entities;

namespace SalesService.Application.Interfaces.Orders;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);

    Task<List<Order>> GetOrdersByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
