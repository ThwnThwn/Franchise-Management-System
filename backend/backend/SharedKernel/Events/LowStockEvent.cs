namespace SharedKernel.Events;

public class LowStockEvent
{
    public Guid StoreId { get; set; }
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinThreshold { get; set; }
    public string ManagerEmail { get; set; } = string.Empty;
}
