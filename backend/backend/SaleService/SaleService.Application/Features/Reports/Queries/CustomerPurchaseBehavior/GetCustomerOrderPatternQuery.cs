using MediatR;
using Ardalis.Result;
namespace SalesService.Application.Features.Reports.Queries.CustomerPurchaseBehavior;

public sealed record GetCustomerOrderPatternQuery(
Guid StoreId,
DateTimeOffset? FromDate,
DateTimeOffset? ToDate,
int? MinOrders,
int? MaxOrders,
string? SortBy,      // "total_orders" | "avg_order_value" | "last_order_date" | "total_spent"
string? SortOrder,   // "asc" | "desc"
int PageIndex,
int PageSize
) : IRequest<Result<BuildingBlocks.Core.Pagination.PagedResult<CustomerOrderPatternResponse>>>;

public sealed class CustomerOrderPatternResponse
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTimeOffset LastOrderDate { get; set; }
}
