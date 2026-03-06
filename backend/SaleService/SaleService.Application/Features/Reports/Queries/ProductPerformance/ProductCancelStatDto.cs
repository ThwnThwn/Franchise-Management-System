namespace SalesService.Application.Features.Reports.Queries.ProductPerformance;

public sealed class ProductCancelStatDto
{
    public Guid ProductId { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCancelled { get; set; }
    public decimal CancelRate => TotalOrders > 0
        ? Math.Round((decimal)TotalCancelled / TotalOrders * 100, 2)
        : 0;
}
