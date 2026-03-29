using Moq;
using FluentAssertions;
using SalesService.Application.Features.Orders.Queries.GetOrders;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Tests.Features.Orders;

public class GetOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrdersQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrders_WhenQueryIsValid()
    {
        // Arrange
        var query = new GetOrdersQuery(Page: 1, PageSize: 10, Status: null);

        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), Code = "ORD1", TotalAmount = 500, Status = OrderStatus.PaymentPending },
            new Order { Id = Guid.NewGuid(), Code = "ORD2", TotalAmount = 1000, Status = OrderStatus.Completed }
        };

        _orderRepositoryMock.Setup(x => x.GetOrdersAsync(
                query.StoreId, query.Code, query.FromDate, query.ToDate, query.Status,
                query.PaymentMethod, query.Type, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders, 2));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageResult.TotalCount.Should().Be(2);
        result.Value.PageResult.Items.Should().HaveCount(2);
        result.Value.PageResult.Items.First().Code.Should().Be("ORD1");
        result.Value.PageResult.Items.Last().Code.Should().Be("ORD2");

        _orderRepositoryMock.Verify(x => x.GetOrdersAsync(
                query.StoreId, query.Code, query.FromDate, query.ToDate, query.Status,
                query.PaymentMethod, query.Type, query.Page, query.PageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoOrdersMatch()
    {
        // Arrange
        var query = new GetOrdersQuery(Page: 1, PageSize: 10);
        _orderRepositoryMock.Setup(x => x.GetOrdersAsync(
                query.StoreId, query.Code, query.FromDate, query.ToDate, query.Status,
                query.PaymentMethod, query.Type, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Order>(), 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageResult.TotalCount.Should().Be(0);
        result.Value.PageResult.Items.Should().BeEmpty();
    }
}
