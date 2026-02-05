namespace SalesService.Domain.Enums;

public enum OrderStatus
{
    PaymentPending = 0,
    Preparing = 1,
    Ready = 2,
    Completed = 3,
    Cancel = 4
}
