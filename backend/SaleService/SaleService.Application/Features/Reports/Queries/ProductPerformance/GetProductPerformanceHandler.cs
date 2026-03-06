using Ardalis.Result;
using BuildingBlocks.Core.Pagination;
using MediatR;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Features.Reports.Queries.ProductPerformance;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.OrderItems;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Reports.Queries.GetProductPerformance;

public sealed class GetProductPerformanceHandler
    : IRequestHandler<GetProductPerformanceQuery, Result<BuildingBlocks.Core.Pagination.PagedResult<ProductPerformanceResponse>>>
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IProductServiceClient _productServiceClient;

    public GetProductPerformanceHandler(
        IOrderItemRepository orderItemRepository,
        IRefundTicketRepository refundTicketRepository,
        IProductServiceClient productServiceClient)
    {
        _orderItemRepository = orderItemRepository;
        _refundTicketRepository = refundTicketRepository;
        _productServiceClient = productServiceClient;
    }

    public async Task<Result<BuildingBlocks.Core.Pagination.PagedResult<ProductPerformanceResponse>>> Handle(
    GetProductPerformanceQuery request,
    CancellationToken cancellationToken)
    {
        // STEP 1: Resolve products by category
        List<Guid>? filterProductIds = null;
        Dictionary<Guid, ProductInfo> productInfoMap = new();

        if (request.CategoryId.HasValue)
        {
            var categoryProducts = await _productServiceClient
                .GetProductsByCategoryAsync(request.CategoryId.Value);

            if (!categoryProducts.Any())
                return Result.Success(new BuildingBlocks.Core.Pagination.PagedResult<ProductPerformanceResponse>(
                    new List<ProductPerformanceResponse>(), 0,
                    request.PageIndex, request.PageSize));

            productInfoMap = categoryProducts.ToDictionary(p => p.ProductId);
            filterProductIds = categoryProducts.Select(p => p.ProductId).ToList();
        }

        // STEP 2: Query completed order items (revenue + units sold)
        var orderItems = await _orderItemRepository.GetProductSalesStatisticAsync(
            request.StoreId,
            request.FromDate,
            request.ToDate,
            filterProductIds,
            cancellationToken);

        if (!orderItems.Any())
            return Result.Success(new BuildingBlocks.Core.Pagination.PagedResult<ProductPerformanceResponse>(
                new List<ProductPerformanceResponse>(), 0,
                request.PageIndex, request.PageSize));

        // STEP 3: Enrich product info nếu không filter category
        if (!request.CategoryId.HasValue)
        {
            var distinctProductIds = orderItems
                .Select(oi => oi.ProductId)
                .Distinct()
                .ToList();

            var productInfos = await _productServiceClient
                .GetProductsByIdsAsync(distinctProductIds);

            productInfoMap = productInfos.ToDictionary(p => p.ProductId);
        }

        // ✅ STEP 4: Cancel stats 
        var productIds = orderItems.Select(oi => oi.ProductId).Distinct().ToList();

        var cancelStats = await _orderItemRepository.GetProductCancelStatisticAsync(
            request.StoreId,
            request.FromDate,
            request.ToDate,
            productIds,
            cancellationToken);

        var cancelMap = cancelStats.ToDictionary(c => c.ProductId);

        // STEP 5: Group + map
        var items = orderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g =>
            {
                var productId = g.Key;
                productInfoMap.TryGetValue(productId, out var productInfo);
                cancelMap.TryGetValue(productId, out var cancelStat);

                var unitsSold = g.Sum(x => x.Quantity);
                var revenue = g.Sum(x => x.Quantity * x.UnitPrice);

                return new ProductPerformanceResponse
                {
                    ProductId = productId,
                    ProductName = productInfo?.ProductName ?? g.First().ProductName,
                    CategoryId = productInfo.CategoryId,
                    CategoryName = productInfo?.CategoryName,
                    UnitsSold = unitsSold,
                    Revenue = revenue,
                    TotalOrders = cancelStat?.TotalOrders ?? 0,
                    TotalCancelled = cancelStat?.TotalCancelled ?? 0,
                    CancelledRate = cancelStat?.CancelRate ?? 0  
                };
            }).ToList();

        // STEP 6: Tag top/bottom
        TagPerformers(items);

        // STEP 7: Sort
        var sorted = ApplySort(items, request.SortBy, request.SortOrder);

        // STEP 8: Paginate
        var paged = sorted
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result.Success(new BuildingBlocks.Core.Pagination.PagedResult<ProductPerformanceResponse>(
            paged, items.Count, request.PageIndex, request.PageSize));
    }

    // ===== PRIVATE HELPERS =====

    private static void TagPerformers(List<ProductPerformanceResponse> items)
    {
        if (!items.Any()) return;

        var count = items.Count;
        var topCount = Math.Max(1, count / 10);     // top 10%
        var bottomCount = Math.Max(1, count / 10);  // bottom 10%

        var topThreshold = items
            .OrderByDescending(x => x.Revenue)
            .Take(topCount)
            .Last().Revenue;

        var bottomThreshold = items
            .OrderBy(x => x.Revenue)
            .Take(bottomCount)
            .Last().Revenue;

        foreach (var item in items)
        {
            item.Tag = item.Revenue >= topThreshold ? PerformanceTag.Top
                     : item.Revenue <= bottomThreshold ? PerformanceTag.Bottom
                     : PerformanceTag.Normal;
        }
    }

    private static List<ProductPerformanceResponse> ApplySort(
        List<ProductPerformanceResponse> items,
        string? sortBy,
        string? sortOrder)
    {
        var isDesc = sortOrder?.ToLower() != "asc";

        return sortBy?.ToLower() switch
        {
            "units_sold" => isDesc ? items.OrderByDescending(x => x.UnitsSold).ToList()
                                    : items.OrderBy(x => x.UnitsSold).ToList(),

            "cancel_rate" => isDesc ? items.OrderByDescending(x => x.CancelledRate).ToList()  
                                : items.OrderBy(x => x.CancelledRate).ToList(),

            "category" => isDesc ? items.OrderByDescending(x => x.CategoryName).ToList()
                                    : items.OrderBy(x => x.CategoryName).ToList(),

            _ => isDesc ? items.OrderByDescending(x => x.Revenue).ToList()
                                    : items.OrderBy(x => x.Revenue).ToList()
        };
    }
}