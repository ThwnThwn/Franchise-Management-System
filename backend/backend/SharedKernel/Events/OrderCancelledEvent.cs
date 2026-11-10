namespace SharedKernel.Events;

public class OrderCancelledEvent
{
    public Guid OrderId { get; set; }
    public DateTimeOffset CancelledAt { get; set; }
    public string Reason { get; set; } = string.Empty;
}
