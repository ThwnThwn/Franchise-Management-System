namespace WarehouseService.Domain.Enums
{
    public enum TransferStatus
    {
        Draft = 1,
        Pending = 2,
        Approved = 3,
        InTransit = 4,
        Completed = 5,
        Cancelled = 6,
        Rejected = 7
    }
}
