using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.Commands.UpdateOrderStatus
{
    public static class OrderStatusTransitionPolicy
    {
        private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions =
            new()
            {
                [OrderStatus.PaymentPending] = new[]
                {
                    OrderStatus.ConfirmationPending,
                    OrderStatus.Cancelled
                },

                [OrderStatus.ConfirmationPending] = new[]
                {
                    OrderStatus.Confirmed,
                    OrderStatus.Cancelled
                },

                [OrderStatus.Confirmed] = new[]
                {
                    OrderStatus.Preparing
                },

                [OrderStatus.Preparing] = new[]
                {
                    OrderStatus.ReadyForPickup
                },

                [OrderStatus.ReadyForPickup] = new[]
                {
                    OrderStatus.Delivering,
                    OrderStatus.Completed
                },

                [OrderStatus.Delivering] = new[]
                {
                    OrderStatus.Completed
                },

                [OrderStatus.Completed] = Array.Empty<OrderStatus>(),
                [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
            };

        public static bool CanTransition(
            OrderStatus currentStatus,
            OrderStatus nextStatus)
        {
            return AllowedTransitions.TryGetValue(currentStatus, out var allowed) &&
                   allowed.Contains(nextStatus);
        }
    }
}