using System.Threading;
using System.Threading.Tasks;
using SalesService.Domain.Entities;

namespace SalesService.Application.Interfaces.Orders;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
