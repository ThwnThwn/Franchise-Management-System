namespace SalesService.Domain.Enums;

public enum WithdrawalStatus
{
    Unspecified = 0,
    Pending = 1,
    Approved = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5,
    Rejected = 6,
    Cancelled = 7
}