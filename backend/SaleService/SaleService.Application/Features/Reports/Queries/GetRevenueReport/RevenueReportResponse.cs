using System;
using System.Collections.Generic;

namespace SalesService.Application.Features.Reports.Queries.GetRevenueReport;

public class RevenueReportResponse
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public Guid? CategoryId { get; set; }

    public Guid? StoreId { get; set; }
    public List<RevenueItemDto> Items { get; set; } = new();
    
    public decimal OverallTotalRevenue { get; set; }
}
