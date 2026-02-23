using Ardalis.Result;
using MediatR;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Persisitences.Orders;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Features.Reports.Queries.GetRevenueReport;

public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, Result<RevenueReportResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductServiceClient _productServiceClient;

    public GetRevenueReportQueryHandler(IOrderRepository orderRepository, IProductServiceClient productServiceClient)
    {
        _orderRepository = orderRepository;
        _productServiceClient = productServiceClient;
    }


    /// <summary>
    /// xử lý truy vấn báo cáo doanh thu, nếu có categoryId thì sẽ lấy danh sách SKU của category đó từ ProductService để lọc dữ liệu doanh thu, nếu không có categoryId thì sẽ lấy tất cả doanh thu trong khoảng thời gian đã cho
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<RevenueReportResponse>> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        List<string>? categorySkus = null;

        // If category is provided, fetch SKUs for that category from ProductService
        if (request.CategoryId.HasValue)
        {
            categorySkus = await _productServiceClient.GetSkusByCategoryAsync(request.CategoryId.Value);

            // If the category has no SKUs, return empty report early
            if (categorySkus == null || !categorySkus.Any())
            {
                return Result<RevenueReportResponse>.Success(new RevenueReportResponse
                {
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    CategoryId = request.CategoryId,
                    Items = new List<RevenueItemDto>(),
                    OverallTotalRevenue = 0
                });
            }
        }

        // Query the repository for revenue data
        var revenueItems = await _orderRepository.GetRevenueReportAsync(
            request.FromDate,
            request.ToDate,
            categorySkus,
            cancellationToken);

        var response = new RevenueReportResponse
        {
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            CategoryId = request.CategoryId,
            Items = revenueItems,
            OverallTotalRevenue = revenueItems.Sum(x => x.TotalRevenue)
        };

        return Result<RevenueReportResponse>.Success(response);
    }
}
