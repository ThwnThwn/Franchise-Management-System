using System.Threading;
using System.Threading.Tasks;
using SaleService.Domain.Entities;

namespace SaleService.Application.Interfaces.Orders;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
}
