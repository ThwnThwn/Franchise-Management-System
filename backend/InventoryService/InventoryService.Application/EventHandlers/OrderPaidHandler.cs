using System.Threading.Tasks;
using InventoryService.Application.Features.StoreIngredient.Commands.ConfirmStockReservation;
using MediatR;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace InventoryService.Application.EventHandlers;

/// <summary>
/// Handler xử lý sự kiện khi đơn hàng đã thanh toán thành công.
/// Nhiệm vụ:
/// - Kích hoạt command trừ kho (ConfirmStockReservation).
/// </summary>
public class OrderPaidHandler : IIntegrationEventHandler<OrderPaidEvent>
{
    private readonly IMediator _mediator;

    public OrderPaidHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderPaidEvent @event)
    {
        var command = new ConfirmStockReservationCommand(
            @event.OrderId,
            @event.StoreId,
            @event.Items);

        await _mediator.Send(command);
    }
}
