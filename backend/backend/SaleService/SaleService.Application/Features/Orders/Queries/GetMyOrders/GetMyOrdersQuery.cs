using MediatR;

namespace SalesService.Application.Features.Orders.Queries.GetMyOrders
{
    public record GetMyOrdersQuery(Guid CustomerId, int Page = 1, int PageSize = 10) : IRequest<GetMyOrdersResponse>;
}
