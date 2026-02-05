using SalesService.Domain.Entities;

namespace SalesService.Application.Persisitences.Orders;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Order>> GetOrdersByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}