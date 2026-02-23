namespace NotificationService.Domain.Enums;

public enum NotificationType
{
    // Auth
    Otp = 1,
    PasswordReset = 2,

    // Order lifecycle
    OrderCreated = 3,        // Order placed (ConfirmationPending)
    OrderConfirmed = 5,      // Confirmed
    OrderPreparing = 6,      // Preparing
    OrderReadyForPickup = 7, // ReadyForPickup
    OrderDelivering = 8,     // Delivering
    OrderCompleted = 9,      // Completed
    OrderCancelled = 10,     // Cancelled

    // Inventory
    LowStock = 4
}