using SharedKernel.Events;

namespace SalesService.Application.Mapping.Orders
{
    public static class OrderToKafka
    {
        public static OrderStatusEvent.OrderStatus MapOrderStatus(
            SalesService.Domain.Enums.OrderStatus status)
        {
            return status switch
            {
                SalesService.Domain.Enums.OrderStatus.PaymentPending
                    => OrderStatusEvent.OrderStatus.PaymentPending,
                SalesService.Domain.Enums.OrderStatus.ConfirmationPending
                    => OrderStatusEvent.OrderStatus.ConfirmationPending,
                SalesService.Domain.Enums.OrderStatus.Confirmed
                    => OrderStatusEvent.OrderStatus.Confirmed,
                SalesService.Domain.Enums.OrderStatus.Preparing
                    => OrderStatusEvent.OrderStatus.Preparing,
                SalesService.Domain.Enums.OrderStatus.ReadyForPickup
                    => OrderStatusEvent.OrderStatus.ReadyForPickup,
                SalesService.Domain.Enums.OrderStatus.Delivering
                    => OrderStatusEvent.OrderStatus.Delivering,
                SalesService.Domain.Enums.OrderStatus.Completed
                    => OrderStatusEvent.OrderStatus.Completed,
                SalesService.Domain.Enums.OrderStatus.Cancelled
                    => OrderStatusEvent.OrderStatus.Cancelled,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        public static List<OrderStatusEvent.CartItemDto> MapOrderItems(
            IEnumerable<SalesService.Domain.Entities.OrderItem> items)
        {
            return items.Select(i => new OrderStatusEvent.CartItemDto
            {
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
            }).ToList();
        }
    }
}