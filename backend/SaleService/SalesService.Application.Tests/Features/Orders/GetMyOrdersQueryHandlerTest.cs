using Moq;
using FluentAssertions;
using SalesService.Application.Features.Orders.Queries.GetMyOrders;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Tests.Features.Orders;

public class GetMyOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly GetMyOrdersQueryHandler _handler;

    public GetMyOrdersQueryHandlerTests()
    {
        _handler = new GetMyOrdersQueryHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrders_WhenCustomerHasOrders()
    {
        var customerId = Guid.NewGuid();
        var query = new GetMyOrdersQuery(customerId, 1, 10);
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), Code = "ORD1", CustomerId = customerId, TotalAmount = 500, Status = OrderStatus.Completed },
            new() { Id = Guid.NewGuid(), Code = "ORD2", CustomerId = customerId, TotalAmount = 300, Status = OrderStatus.PaymentPending }
        };
        _orderRepoMock.Setup(x => x.GetOrdersByCustomerIdAsync(customerId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(orders);
        _orderRepoMock.Setup(x => x.CountOrdersByCustomerIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Orders.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoOrders()
    {
        var customerId = Guid.NewGuid();
        var query = new GetMyOrdersQuery(customerId, 1, 10);
        _orderRepoMock.Setup(x => x.GetOrdersByCustomerIdAsync(customerId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Order>());
        _orderRepoMock.Setup(x => x.CountOrdersByCustomerIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Orders.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
