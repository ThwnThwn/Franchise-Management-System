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
    string? SortBy,        // "revenue" | "units_sold" | "cancel_rate"
    string? SortOrder,     // "asc" | "desc"
    int PageIndex,
    int PageSize
) : IRequest<Result<BuildingBlocks.Core.Pagination.PagedResult<ProductPerformanceResponse>>>;

// Response
public sealed class ProductPerformanceResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
    public decimal CancelledRate { get; set; }       // % số đơn bị cancel
    public int TotalOrders { get; set; }
    public int TotalCancelled { get; set; }
    public PerformanceTag Tag { get; set; }        // Top ban chay nhat| Bottom ban it nhat | Normal
}


