namespace SupplierService.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdResult
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public int Status { get; set; }
    public int PaymentStatus { get; set; }
    public double TotalAmount { get; set; }
    public double DiscountAmount { get; set; }
    public double NetAmount { get; set; }
    public double PaidAmount { get; set; }
    public string OrderDate { get; set; } = string.Empty;
    public string CompletedAt { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public IEnumerable<PurchaseOrderItemDetailDto> Items { get; set; } = new List<PurchaseOrderItemDetailDto>();
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

public class PurchaseOrderItemDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string IngredientId { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double UnitPrice { get; set; }
    public double TotalPrice { get; set; }
}
