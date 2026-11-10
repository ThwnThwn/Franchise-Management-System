namespace SalesService.Domain.Enums;

public enum OrderStatus
{
    PaymentPending = 0,
    ConfirmationPending = 1,
    Confirmed = 2,
    Preparing = 3,
    ReadyForPickup = 4,
    Delivering = 5,
    Completed = 6,
    Cancelled = 7
}
