using SalesService.Application.Features.Reports.Queries.ProductPerformance;
using SalesService.Domain.Entities;

namespace SalesService.Application.Persisitences.OrderItems
{
    public interface IOrderItemRepository
    {
        void RemoveOrderItems(List<OrderItem> items);
        void AddOrderItems(List<OrderItem> items);
        Task SaveAsync(CancellationToken cancellationToken = default);

        Task<List<ProductCancelStatDto>> GetProductCancelStatisticAsync(
        Guid storeId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        List<Guid>? productIds,
        CancellationToken ct);

       Task<List<OrderItem>> GetProductSalesStatisticAsync(
            Guid storeId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            List<Guid>? productIds,
            CancellationToken ct);
    }
}

