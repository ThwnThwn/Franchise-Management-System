namespace SupplierService.Domain.Enums;

public enum PurchaseOrderStatus
{
    Pending = 0,
    ConfirmPending = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3,
    Cancelled = 4,
    Partial = 5
}

