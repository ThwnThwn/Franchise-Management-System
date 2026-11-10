using System;

namespace SalesService.Application.Features.Reports.Queries.GetRevenueReport;

public class RevenueItemDto
{
    public Guid? StoreId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalRevenue { get; set; }
}
