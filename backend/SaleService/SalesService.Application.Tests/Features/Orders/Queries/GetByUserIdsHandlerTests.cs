using Moq;
using FluentAssertions;
using SalesService.Application.Features.Orders.Queries.GetByIds;
using SalesService.Application.Features.Orders.Queries.GetByUserIds;
using SalesService.Application.Persisitences.Orders;

namespace SalesService.Application.Tests.Features.Orders.Queries.GetByUserIds;

public class GetByUserIdsHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly GetByUserIdsHandler _handler;

    public GetByUserIdsHandlerTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _handler = new GetByUserIdsHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserIdsIsNull()
    {
        // Arrange
        var query = new GetByUserIdsQuery { UserIds = null! };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _orderRepoMock.Verify(x => x.GetByUserIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserIdsIsEmpty()
    {
        // Arrange
        var query = new GetByUserIdsQuery { UserIds = new List<Guid>() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _orderRepoMock.Verify(x => x.GetByUserIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnData_WhenUserIdsAreProvided()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = new GetByUserIdsQuery { UserIds = userIds };
        var mockResult = new List<GetByUserIdsRes>
        {
            new GetByUserIdsRes { UserId = userIds[0], OrderCount = 5 },
            new GetByUserIdsRes { UserId = userIds[1], OrderCount = 3 }
        };

        _orderRepoMock.Setup(x => x.GetByUserIdsAsync(userIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(mockResult);
        _orderRepoMock.Verify(x => x.GetByUserIdsAsync(userIds, It.IsAny<CancellationToken>()), Times.Once);
    }
}
