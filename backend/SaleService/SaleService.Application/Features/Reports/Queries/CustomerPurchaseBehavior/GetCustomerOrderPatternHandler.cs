using MediatR;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.Orders;
using Ardalis.Result;

namespace SalesService.Application.Features.Reports.Queries.CustomerPurchaseBehavior;

public sealed class GetCustomerOrderPatternHandler
: IRequestHandler<GetCustomerOrderPatternQuery, Result<BuildingBlocks.Core.Pagination.PagedResult<CustomerOrderPatternResponse>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public GetCustomerOrderPatternHandler(
        IOrderRepository orderRepository,
        IIdentityServiceClient identityServiceClient)
    {
        _orderRepository = orderRepository;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<BuildingBlocks.Core.Pagination.PagedResult<CustomerOrderPatternResponse>>> Handle(
        GetCustomerOrderPatternQuery request,
        CancellationToken cancellationToken)
    {
        //  Lấy stats từ DB
        var stats = await _orderRepository.GetCustomerOrderStatisticAsync(
            request.StoreId,
            request.FromDate,
            request.ToDate,
            request.MinOrders,
            request.MaxOrders,
            cancellationToken);

        if (!stats.Any())
            return Result.Success(new BuildingBlocks.Core.Pagination.PagedResult<CustomerOrderPatternResponse>(
                new List<CustomerOrderPatternResponse>(), 0,
                request.PageIndex, request.PageSize));

        //  Enrich từ IdentityService
        var customerIds = stats.Select(s => s.CustomerId).ToList();
        var customerInfos = await _identityServiceClient.GetUsersByIdsAsync(customerIds);
        var customerMap = customerInfos.ToDictionary(c => c.Id);

        // Map
        var items = stats.Select(s =>
        {
            customerMap.TryGetValue(s.CustomerId, out var info);
            return new CustomerOrderPatternResponse
            {
                CustomerId = s.CustomerId,
                FullName = info?.Username,
                Email = info?.Email,
                Phone = info?.Phone,
                TotalOrders = s.TotalOrders,
                AverageOrderValue = s.AverageOrderValue,
                TotalSpent = s.TotalSpent,
                LastOrderDate = s.LastOrderDate
            };
        }).ToList();

        // Sort
        items = ApplySort(items, request.SortBy, request.SortOrder);

        //  Paginate
        var paged = items
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result.Success(new BuildingBlocks.Core.Pagination.PagedResult<CustomerOrderPatternResponse>(
            paged, items.Count, request.PageIndex, request.PageSize));
    }

    private static List<CustomerOrderPatternResponse> ApplySort(
        List<CustomerOrderPatternResponse> items,
        string? sortBy,
        string? sortOrder)
    {
        var isDesc = sortOrder?.ToLower() != "asc";

        return sortBy?.ToLower() switch
        {
            "avg_order_value" => isDesc ? items.OrderByDescending(x => x.AverageOrderValue).ToList()
                                         : items.OrderBy(x => x.AverageOrderValue).ToList(),
            "last_order_date" => isDesc ? items.OrderByDescending(x => x.LastOrderDate).ToList()
                                         : items.OrderBy(x => x.LastOrderDate).ToList(),
            "total_spent" => isDesc ? items.OrderByDescending(x => x.TotalSpent).ToList()
                                         : items.OrderBy(x => x.TotalSpent).ToList(),
            _ => isDesc ? items.OrderByDescending(x => x.TotalOrders).ToList()  // default: total_orders
                                         : items.OrderBy(x => x.TotalOrders).ToList()
        };
    }
}
