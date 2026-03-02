using MediatR;
using SalesService.Application.Features.Orders.Queries.GetByIds;
using SalesService.Application.Persisitences.Orders;

namespace SalesService.Application.Features.Orders.Queries.GetByUserIds
{
    public class GetByUserIdsHandler : IRequestHandler<GetByUserIdsQuery, IEnumerable<GetByUserIdsRes>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetByUserIdsHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<GetByUserIdsRes>> Handle(
            GetByUserIdsQuery request,
            CancellationToken cancellationToken)
        {
            if (request.UserIds == null || !request.UserIds.Any())
                return new List<GetByUserIdsRes>();

            return await _orderRepository.GetByUserIdsAsync(request.UserIds, cancellationToken);
        }
    }
}