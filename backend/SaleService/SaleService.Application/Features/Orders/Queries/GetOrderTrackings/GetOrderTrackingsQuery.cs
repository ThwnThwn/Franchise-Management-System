using Ardalis.Result;
using MediatR;

namespace SalesService.Application.Features.Orders.Queries.GetOrderTrackings;

public record GetOrderTrackingsQuery(
    Guid? StoreId,
    Guid? OrderId,
    int PageNumber,
    int PageSize
) : IRequest<Result<GetOrderTrackingsResponse>>;

public class GetOrderTrackingsResponse
{
    public List<OrderTrackingDto> Trackings { get; set; } = new();
    public int TotalCount { get; set; }
}

public class OrderTrackingDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
