using BuildingBlocks.Core.Pagination;
using SalesService.Application.Features.Orders.Queries.GetMyOrders;

namespace SalesService.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersResponse(PagedResult<OrderSummaryDto> PageResult);