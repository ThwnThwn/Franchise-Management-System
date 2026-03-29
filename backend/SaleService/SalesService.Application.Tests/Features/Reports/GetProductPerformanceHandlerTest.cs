using Moq;
using FluentAssertions;
using SalesService.Application.Features.Reports.Queries.GetProductPerformance;
using SalesService.Application.Features.Reports.Queries.ProductPerformance;
using SalesService.Application.Persisitences.OrderItems;
using SalesService.Application.ClientInterface.Products;
using SalesService.Domain.Entities;

namespace SalesService.Application.Tests.Features.Reports;

public class GetProductPerformanceHandlerTests
{
    private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
    private readonly Mock<IProductServiceClient> _productServiceClientMock;
    private readonly GetProductPerformanceHandler _handler;

    public GetProductPerformanceHandlerTests()
    {
        _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
        _productServiceClientMock = new Mock<IProductServiceClient>();
        _handler = new GetProductPerformanceHandler(_orderItemRepositoryMock.Object, _productServiceClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoSalesExist()
    {
        // Arrange
        var query = new GetProductPerformanceQuery(Guid.NewGuid(), null, null, null, null, null, 1, 10);
        
        _orderItemRepositoryMock.Setup(x => x.GetProductSalesStatisticAsync(
                query.StoreId, query.FromDate, query.ToDate, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderItem>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(0); // PagedResult has TotalCount
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnPerformance_WhenDataExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var query = new GetProductPerformanceQuery(storeId, null, null, null, "revenue", "desc", 1, 10);

        var orderItems = new List<OrderItem>
        {
            new OrderItem { ProductId = productId, ProductName = "Test Product", Quantity = 2, UnitPrice = 50m }
        };

        var cancelStats = new List<ProductCancelStatDto>
        {
            new ProductCancelStatDto { ProductId = productId, TotalOrders = 5, TotalCancelled = 1 }
        };

        var productInfos = new List<ProductInfo>
        {
            new ProductInfo { ProductId = productId, ProductName = "Test Product", CategoryId = categoryId, CategoryName = "Category 1" }
        };

        _orderItemRepositoryMock.Setup(x => x.GetProductSalesStatisticAsync(
                storeId, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItems);

        _orderItemRepositoryMock.Setup(x => x.GetProductCancelStatisticAsync(
                storeId, null, null, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelStats);

        _productServiceClientMock.Setup(x => x.GetProductsByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(productInfos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.Should().HaveCount(1);
        var item = result.Value.Items.First();
        item.ProductId.Should().Be(productId);
        item.UnitsSold.Should().Be(2);
        item.Revenue.Should().Be(100m); // 2 * 50
        item.TotalOrders.Should().Be(5);
        item.TotalCancelled.Should().Be(1);
        item.CancelledRate.Should().Be(20m);
    }
}
