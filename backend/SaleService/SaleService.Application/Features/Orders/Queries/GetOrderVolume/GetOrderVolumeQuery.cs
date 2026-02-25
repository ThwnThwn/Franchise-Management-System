using MediatR;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.Queries.GetOrderVolume
{
    public record class GetOrderVolumeQuery(GetOrderVolumeRequest Request) : IRequest<GetOrderVolumeResponse>;
    public class GetOrderVolumeRequest
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public Guid? StoreId { get; set; }

        public OrderStatus? Status { get; set; }
        public bool? Include_csv { get; set; }
    }
    public class GetOrderVolumeResponse
    {
        public List<OrderVolumePerDayDto> Orders { get; set; } = new();
        public byte[]? CsvFile { get; init; }
        public string? CsvFileName { get; init; }
    }
    public class OrderVolumePerDayDto
    {
        public DateOnly Date { get; set; }
        public Guid? StoreId { get; set; }
        public int TotalOrders { get; set; }

        public int CanceledOrders { get; set; }

        public int CompletedOrders { get; set; }
        public Dictionary<OrderStatus, int> StatusCounts { get; set; } = new();
    }
}

