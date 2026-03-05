namespace SalesService.Application.Features.Reports.Queries.GetSalesTrend;

public class SalesTrendDataPointDto
{
    public string TimeLabel { get; set; } = string.Empty;
    public int OrderVolume { get; set; }
    public decimal Revenue { get; set; }
}
