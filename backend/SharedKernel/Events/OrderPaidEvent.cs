namespace SharedKernel.Events;

public class OrderPaidEvent
{
    public Guid OrderId { get; set; }
    public Guid StoreId { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public List<OrderPaidItem> Items { get; set; } = new();
}

public class OrderPaidItem
{
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
}
