using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQuery(
    Guid? StoreId = null,
    string? Code = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    OrderStatus? Status = null,
    PaymentMethod? PaymentMethod = null,
    OrderType? Type = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<GetOrdersResponse>>;