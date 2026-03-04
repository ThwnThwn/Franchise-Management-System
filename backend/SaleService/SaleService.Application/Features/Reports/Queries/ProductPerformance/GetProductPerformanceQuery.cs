using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
namespace SalesService.Application.Features.Reports.Queries.ProductPerformance;

// Query
public sealed record GetProductPerformanceQuery(
    Guid StoreId,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    Guid? CategoryId,
    string? SortBy,        // "revenue" | "units_sold" | "return_rate"
    string? SortOrder,     // "asc" | "desc"
    int PageIndex,
    int PageSize
) : IRequest<Result<PagedResult<ProductPerformanceResponse>>>;

// Response
public sealed class ProductPerformanceResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? CategoryName { get; set; }
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
    public decimal ReturnRate { get; set; }       // % số đơn bị refund
    public int TotalOrders { get; set; }
    public int TotalRefunds { get; set; }
    public PerformanceTag Tag { get; set; }        // Top ban chay nhat| Bottom ban it nhat | Normal
}


