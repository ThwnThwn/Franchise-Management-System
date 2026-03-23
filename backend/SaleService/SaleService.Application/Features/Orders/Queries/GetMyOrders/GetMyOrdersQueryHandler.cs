using MediatR;
using SalesService.Application.Persisitences.Orders;

namespace SalesService.Application.Features.Orders.Queries.GetMyOrders
{
    public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, GetMyOrdersResponse>
    {
        private readonly IOrderRepository _orderRepository;

        public GetMyOrdersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<GetMyOrdersResponse> Handle(
            GetMyOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(
                request.CustomerId,
                request.Page,
                request.PageSize,
                cancellationToken);

            var totalCount = await _orderRepository.CountOrdersByCustomerIdAsync(
                request.CustomerId,
                cancellationToken);

            // Map to DTO
            var orderDtos = orders.Select(o => new OrderSummaryDto(
                o.Id,
                o.Code,
                o.Subtotal,
                o.ShippingFee,
                o.Tax,
                o.TotalAmount,
                o.PaidAmount,
                (int?)o.PaymentMethod,
                (int?)o.Type,
                (int)o.Status,
                o.PaidAt,
                o.Notes,
                o.CreatedAt,
                o.UpdatedAt,
                o.CustomerId,
                o.StoreId,
                DelivererId: o.DelivererId,
                o.ScheduledTime,
                o.Address,
                o.Latitude,
                o.Longitude,
                o.Distance
            )).ToList();
            return new GetMyOrdersResponse(orderDtos, totalCount);
        }
    }
}