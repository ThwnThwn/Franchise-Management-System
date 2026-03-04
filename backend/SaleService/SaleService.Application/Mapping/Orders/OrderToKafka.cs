using SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;
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


        /// <summary>
        /// Map Order → OrderPaidEvent để publish lên Kafka cho InventoryService trừ kho.
        /// </summary>
        public static OrderPaidEvent MapToOrderPaidEvent(SalesService.Domain.Entities.Order order)
        {
            return new OrderPaidEvent
            {
                OrderId = order.Id,
                StoreId = order.StoreId ?? Guid.Empty,
                PaidAt = DateTimeOffset.UtcNow,
                Items = order.OrderItems?.Select(oi => new OrderPaidItem
                {
                    VariantId = oi.VariantId,
                    Quantity = oi.Quantity
                }).ToList() ?? new List<OrderPaidItem>()
            };
        }

        /// <summary>
        /// Map Order → OrderStatusEvent cho NotificationService (thông báo).
        /// </summary>
        public static OrderStatusEvent MapToOrderStatusEvent(SalesService.Domain.Entities.Order order)
        {
            return new OrderStatusEvent
            {
                OrderId = order.Id,
                OrderCode = order.Code,
                CustomerId = order.CustomerId,
                Title = DictionaryOrderTileEmail.GetTitle(order.Status),
                CustomerEmail = string.Empty,
                NewOrderStatus = MapOrderStatus(order.Status),
                Items = MapOrderItems(order.OrderItems ?? []),
                OrderSummary = $"Order {order.Code} created with {order.OrderItems?.Count ?? 0} items",
                Tax = order.Tax,
                TotalAmount = order.TotalAmount,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
    }
}