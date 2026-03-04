using SalesService.Application.Features.Orders.Queries.GetByIds;
using SalesService.Application.Features.Orders.Queries.GetOrderVolume;
using SalesService.Application.Features.Reports.Queries.GetRevenueReport;
using SalesService.Application.Features.Reports.Queries.GetSalesTrend;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using System.Collections.Generic;

namespace SalesService.Application.Persisitences.Orders;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Order>> GetOrdersByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<GetByUserIdsRes>> GetByUserIdsAsync(List<Guid> userIds, CancellationToken cancellationToken = default);
    Task<int> CountOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<(List<Order> Items, int TotalCount)> GetOrdersAsync(
        Guid? storeId,
        string? code,
        DateOnly? fromDate,
        DateOnly? toDate,
        OrderStatus? status,
        PaymentMethod? paymentMethod,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);

    Task<List<RevenueItemDto>> GetRevenueReportAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        List<string>? skus,
        Guid? storeId,
        CancellationToken cancellationToken = default);

    Task<List<OrderVolumePerDayDto>> GetOrderVolumeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        Guid? storeId,
        OrderStatus? status,
        CancellationToken cancellationToken);

    Task<List<SalesTrendDataPointDto>> GetSalesTrendAsync(
        DateTimeOffset fromDate,
        DateTimeOffset toDate,
        TimeAggregation aggregation,
        List<string>? categorySkus,
        Guid? storeId,
        CancellationToken cancellationToken = default);
}