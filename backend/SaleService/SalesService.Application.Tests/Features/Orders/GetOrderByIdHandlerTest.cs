using Moq;
using FluentAssertions;
using SalesService.Application.Features.Orders.Queries.GetById;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Tests.Features.Orders;

public class GetOrderByIdHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrderByIdQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenOrderIdIsEmpty()
    {
        // Arrange
        var query = new GetOrderByIdQuery(Guid.Empty);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "OrderId is required.");
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var query = new GetOrderByIdQuery(Guid.NewGuid());
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(query.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderIsDeleted()
    {
        // Arrange
        var query = new GetOrderByIdQuery(Guid.NewGuid());
        var order = new Order { Id = query.OrderId, IsDeleted = true };
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(query.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOrderExists()
    {
        // Arrange
        var query = new GetOrderByIdQuery(Guid.NewGuid());
        var order = new Order
        {
            Id = query.OrderId,
            Code = "ORD-001",
            TotalAmount = 1000m,
            PaymentMethod = PaymentMethod.VNPay,
            Status = OrderStatus.Completed,
            OrderItems = new List<OrderItem>
            {
                new OrderItem { Id = Guid.NewGuid(), Quantity = 2, UnitPrice = 500m, ProductName = "Test Item" }
            },
            OrderSaleItems = new List<OrderSaleItem>
            {
                new OrderSaleItem { Id = Guid.NewGuid(), Amount = -100m, Type = OrderSaleItemType.Voucher }
            }
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(query.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(query.OrderId);
        result.Value.OrderCode.Should().Be("ORD-001");
        result.Value.TotalAmount.Should().Be(1000m);
        result.Value.Status.Should().Be((int)OrderStatus.Completed);
        result.Value.PaymentMethod.Should().Be((int)PaymentMethod.VNPay);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().ProductName.Should().Be("Test Item");
        result.Value.SaleItems.Should().HaveCount(1);
    }
}
