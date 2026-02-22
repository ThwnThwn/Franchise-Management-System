using SalesService.Application.Features.Orders.Queries.GetMyOrders;

namespace SalesService.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersResponse(List<OrderSummaryDto> Orders, int TotalCount);
