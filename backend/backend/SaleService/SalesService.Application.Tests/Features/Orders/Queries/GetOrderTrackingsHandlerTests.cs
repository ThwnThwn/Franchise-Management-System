using Moq;
using FluentAssertions;
using SalesService.Application.Features.Orders.Queries.GetOrderTrackings;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Tests.Features.Orders.Queries;

public class GetOrderTrackingsHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly GetOrderTrackingsHandler _handler;

    public GetOrderTrackingsHandlerTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _handler = new GetOrderTrackingsHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoData()
    {
        // Arrange
        var query = new GetOrderTrackingsQuery(null, null, 1, 10);
        _orderRepoMock.Setup(x => x.GetOrderTrackingsAsync(
            It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<OrderTracking>(), 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Trackings.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoMappedCorrectly()
    {
        // Arrange
        var query = new GetOrderTrackingsQuery(Guid.NewGuid(), Guid.NewGuid(), 1, 10);
        var order = new Order { Code = "ORD-001" };
        var trackings = new List<OrderTracking>
        {
            new OrderTracking { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), Status = OrderStatus.Completed, Action = "Updated", Note = "Test", Order = order, CreatedAt = DateTimeOffset.UtcNow }
        };

        _orderRepoMock.Setup(x => x.GetOrderTrackingsAsync(
            query.StoreId, query.OrderId, query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((trackings, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Trackings.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Trackings[0].OrderCode.Should().Be("ORD-001");
        result.Value.Trackings[0].Action.Should().Be("Updated");
        result.Value.Trackings[0].Note.Should().Be("Test");
        result.Value.Trackings[0].Status.Should().Be((int)OrderStatus.Completed);
    }
}
