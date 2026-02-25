using MediatR;
using SalesService.Application.Persisitences.Orders;

namespace SalesService.Application.Features.Orders.Queries.GetOrderVolume
{
    public class GetOrderVolumeHandler
        : IRequestHandler<GetOrderVolumeQuery, GetOrderVolumeResponse>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderVolumeHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<GetOrderVolumeResponse> Handle(GetOrderVolumeQuery request,CancellationToken cancellationToken)
        {
            var data = await _orderRepository.GetOrderVolumeAsync(
                request.Request.FromDate,
                request.Request.ToDate,
                request.Request.StoreId,
                request.Request.Status,
                cancellationToken);

            byte[]? csvBytes = null;
            string? csvFileName = null;
            if (request.Request.Include_csv == true)
            {
                csvBytes = OrderVolumeCsvExporter.Export(data);
                csvFileName = $"order-volume-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            }

            return new GetOrderVolumeResponse
            {
                Orders = data,
                CsvFile = csvBytes,
                CsvFileName = csvFileName
            };
        }
    }
}