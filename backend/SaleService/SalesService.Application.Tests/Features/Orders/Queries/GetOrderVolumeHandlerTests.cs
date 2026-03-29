using FluentAssertions;
using Moq;
using SalesService.Application.Features.Orders.Queries.GetOrderVolume;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Enums;

namespace SalesService.Application.Tests.Features.Orders.Queries;

public class GetOrderVolumeHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly GetOrderVolumeHandler _handler;

    public GetOrderVolumeHandlerTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _handler = new GetOrderVolumeHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrdersOnly_WhenIncludeCsvIsFalse()
    {
        // Arrange
        var request = new GetOrderVolumeQuery(new GetOrderVolumeRequest
        {
            FromDate = new DateOnly(2023, 1, 1),
            ToDate = new DateOnly(2023, 1, 31),
            StoreId = Guid.NewGuid(),
            Status = OrderStatus.Completed,
            Include_csv = false
        });

        var mockOrders = new List<OrderVolumePerDayDto>
        {
            new OrderVolumePerDayDto { Date = new DateOnly(2023, 1, 1), TotalOrders = 10, CompletedOrders = 10 }
        };

        _orderRepoMock.Setup(x => x.GetOrderVolumeAsync(
            request.Request.FromDate,
            request.Request.ToDate,
            request.Request.StoreId,
            request.Request.Status,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockOrders);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().BeEquivalentTo(mockOrders);
        result.CsvFile.Should().BeNull();
        result.CsvFileName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnOrdersAndCsv_WhenIncludeCsvIsTrue()
    {
        // Arrange
        var request = new GetOrderVolumeQuery(new GetOrderVolumeRequest
        {
            FromDate = new DateOnly(2023, 1, 1),
            ToDate = new DateOnly(2023, 1, 31),
            Include_csv = true
        });

        var mockOrders = new List<OrderVolumePerDayDto>
        {
            new OrderVolumePerDayDto { Date = new DateOnly(2023, 1, 1), TotalOrders = 10, CompletedOrders = 8, CanceledOrders = 2 }
        };

        _orderRepoMock.Setup(x => x.GetOrderVolumeAsync(
            request.Request.FromDate,
            request.Request.ToDate,
            null,
            null,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockOrders);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().BeEquivalentTo(mockOrders);
        result.CsvFile.Should().NotBeNull();
        result.CsvFile.Should().NotBeEmpty();
        result.CsvFileName.Should().NotBeNullOrEmpty();
        result.CsvFileName.Should().Contain("order-volume-");
        result.CsvFileName.Should().EndWith(".csv");
    }
}
