using Moq;
using FluentAssertions;
using SalesService.Application.Features.Reports.Queries.CustomerPurchaseBehavior;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.Orders;

namespace SalesService.Application.Tests.Features.Reports;

public class GetCustomerOrderPatternHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IIdentityServiceClient> _identityMock = new();
    private readonly GetCustomerOrderPatternHandler _handler;

    public GetCustomerOrderPatternHandlerTests()
    {
        _handler = new GetCustomerOrderPatternHandler(_orderRepoMock.Object, _identityMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoStats()
    {
        var query = new GetCustomerOrderPatternQuery(Guid.NewGuid(), null, null, null, null, null, null, 1, 10);
        _orderRepoMock.Setup(x => x.GetCustomerOrderStatisticAsync(query.StoreId, query.FromDate, query.ToDate, query.MinOrders, query.MaxOrders, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustomerOrderStatDto>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedStats_WhenStatsExist()
    {
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var query = new GetCustomerOrderPatternQuery(storeId, null, null, null, null, "total_orders", "desc", 1, 10);

        var stats = new List<CustomerOrderStatDto>
        {
            new() { CustomerId = customerId, TotalOrders = 5, AverageOrderValue = 100, TotalSpent = 500, LastOrderDate = DateTime.UtcNow }
        };

        _orderRepoMock.Setup(x => x.GetCustomerOrderStatisticAsync(storeId, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        _identityMock.Setup(x => x.GetUsersByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<UserInfo>
            {
                new() { Id = customerId, Username = "testuser", Email = "test@test.com", Phone = "12345" }
            });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().FullName.Should().Be("testuser");
        result.Value.Items.First().TotalOrders.Should().Be(5);
    }
}
