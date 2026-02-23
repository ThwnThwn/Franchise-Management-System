using MediatR;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Features.Orders.Queries.GetMyOrders;
using Ardalis.Result;

namespace SalesService.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<GetOrdersResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<GetOrdersResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await _orderRepository.GetOrdersAsync(
            request.StoreId,
            request.Code,
            request.FromDate,
            request.ToDate,
            request.Status,
            request.PaymentMethod,
            request.Page,
            request.PageSize,
            cancellationToken);

        var orderDtos = orders.Select(o => new OrderSummaryDto(
            o.Id,
            o.Code,
            o.Subtotal,
            o.Tax,
            o.TotalAmount,
            o.PaidAmount,
            (int?)o.PaymentMethod,
            (int)o.Status,
            o.PaidAt,
            o.Notes,
            o.CreatedAt,
            o.UpdatedAt,
            o.CustomerId
        )).ToList();

        //return Result.Success(new GetOrdersResponse(orderDtos, totalCount));
        return Result.Success(new GetOrdersResponse
        (
            new BuildingBlocks.Core.Pagination.PagedResult<OrderSummaryDto>(
                orderDtos, totalCount, request.Page, request.PageSize
        )));
    }
}