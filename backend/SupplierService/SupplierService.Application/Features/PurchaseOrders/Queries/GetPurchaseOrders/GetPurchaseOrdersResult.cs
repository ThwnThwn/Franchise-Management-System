namespace SupplierService.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;

public class GetPurchaseOrdersResult
{
    public IEnumerable<PurchaseOrderSummaryDto> PurchaseOrders { get; set; } = new List<PurchaseOrderSummaryDto>();
    public int TotalCount { get; set; }
}

public class PurchaseOrderSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public int Status { get; set; }
    public int PaymentStatus { get; set; }
    public double TotalAmount { get; set; }
    public double NetAmount { get; set; }
    public string OrderDate { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}
