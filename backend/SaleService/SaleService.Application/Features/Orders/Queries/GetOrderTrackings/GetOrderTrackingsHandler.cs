using Ardalis.Result;
using MediatR;
using SalesService.Application.Persisitences.Orders;

namespace SalesService.Application.Features.Orders.Queries.GetOrderTrackings;

public class GetOrderTrackingsHandler : IRequestHandler<GetOrderTrackingsQuery, Result<GetOrderTrackingsResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderTrackingsHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<GetOrderTrackingsResponse>> Handle(GetOrderTrackingsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _orderRepository.GetOrderTrackingsAsync(
            request.StoreId,
            request.OrderId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var trackingsDto = items.Select(t => new OrderTrackingDto
        {
            Id = t.Id,
            OrderId = t.OrderId,
            OrderCode = t.Order?.Code ?? string.Empty,
            Status = t.Status.HasValue ? (int)t.Status.Value : -1,
            Action = t.Action,
            Note = t.Note ?? string.Empty,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Result.Success(new GetOrderTrackingsResponse
        {
            Trackings = trackingsDto,
            TotalCount = totalCount
        });
    }
}
