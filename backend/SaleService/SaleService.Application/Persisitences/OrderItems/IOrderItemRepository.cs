using SalesService.Domain.Entities;

namespace SalesService.Application.Persisitences.OrderItems
{
    public interface IOrderItemRepository
    {
        void RemoveOrderItems(List<OrderItem> items);
        void AddOrderItems(List<OrderItem> items);
        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}
