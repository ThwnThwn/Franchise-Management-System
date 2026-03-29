using Moq;
using FluentAssertions;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Features.Reports.Queries.GetRevenueReport;
using SalesService.Application.Persisitences.Orders;

namespace SalesService.Application.Tests.Features.Reports;

public class GetRevenueReportQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IProductServiceClient> _productClientMock = new();
    private readonly GetRevenueReportQueryHandler _handler;

    public GetRevenueReportQueryHandlerTests()
    {
        _handler = new GetRevenueReportQueryHandler(_orderRepoMock.Object, _productClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenCategoryHasNoSkus()
    {
        var query = new GetRevenueReportQuery(null, null, Guid.NewGuid(), null);
        _productClientMock.Setup(x => x.GetSkusByCategoryAsync(query.CategoryId.Value)).ReturnsAsync(new List<string>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.OverallTotalRevenue.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnRevenue_WhenValidRequest()
    {
        var query = new GetRevenueReportQuery(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), DateOnly.FromDateTime(DateTime.UtcNow), null, null);
        var revenueData = new List<RevenueItemDto>
        {
            new() { ProductName = "Product 1", TotalRevenue = 1000, QuantitySold = 5 }
        };

        _orderRepoMock.Setup(x => x.GetRevenueReportAsync(query.FromDate, query.ToDate, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revenueData);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.OverallTotalRevenue.Should().Be(1000);
    }
}
