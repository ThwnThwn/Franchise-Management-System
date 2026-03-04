using System.Collections.Generic;

namespace SalesService.Application.Features.Reports.Queries.GetSalesTrend;

public class SalesTrendResponse
{
    public List<SalesTrendDataPointDto> DataPoints { get; set; } = new();
    
    public string? PeakPeriodLabel { get; set; } 
    public string? SlowPeriodLabel { get; set; } 
}
