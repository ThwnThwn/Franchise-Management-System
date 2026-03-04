using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.Commands.UpdateOrderStatus
{
    public class DictionaryOrderTileEmail
    {
        private static readonly Dictionary<OrderStatus, string> Titles =
            new()
            {
                [OrderStatus.ConfirmationPending] = "Your order has been placed successfully!",
                [OrderStatus.Confirmed] = "Your order has been confirmed!",
                [OrderStatus.Preparing] = "Your order is now preparing!",
                [OrderStatus.ReadyForPickup] = "Your order is ready to pick up!",
                [OrderStatus.Delivering] = "Your order is delivering!",
                [OrderStatus.Completed] = "Thank you for choosing us!",
                [OrderStatus.Cancelled] = "Your order is canclled!"
            };

        public static string GetTitle(OrderStatus status)
        {
            return Titles.TryGetValue(status, out var title)
                ? title
                : "Order Status update";
        }
    }
}
