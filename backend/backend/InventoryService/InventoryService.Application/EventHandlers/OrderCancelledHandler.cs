using System.Threading.Tasks;
using InventoryService.Application.Persistences.Repositories;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace InventoryService.Application.EventHandlers;

/// <summary>
/// Handler xử lý sự kiện khi đơn hàng bị hủy.
/// Nhiệm vụ:
/// - Xóa Queue giữ chỗ hàng (Hoàn kho).
/// </summary>
public class OrderCancelledHandler : IIntegrationEventHandler<OrderCancelledEvent>
{
    private readonly IStoreIngredientRepository _repository;

    public OrderCancelledHandler(IStoreIngredientRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(OrderCancelledEvent @event)
    {
        await _repository.RemoveQueueByOrderIdAsync(@event.OrderId);
    }
}
