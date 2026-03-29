using Moq;
using FluentAssertions;
using SalesService.Application.Dtos.CartItem.Request;
using SalesService.Application.Features.CartItems.Commands.Update;
using SalesService.Application.Interfaces.CartItems;
using SalesService.Domain.Entities;

namespace SalesService.Application.Tests.Features.CartItems;

public class UpdateCartItemCommandHandlerTests
{
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly UpdateCartItemCommandHandler _handler;

    public UpdateCartItemCommandHandlerTests()
    {
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _handler = new UpdateCartItemCommandHandler(_cartItemRepositoryMock.Object);
    }

    // 1. THÀNH CÔNG - Cập nhật số lượng item
    [Fact]
    public async Task Handle_ShouldUpdateQuantity_WhenCartItemExists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var req = new UpdateCartItemReq
        {
            CustomerId = customerId,
            Sku = "SKU-001",
            Quantity = 10
        };
        var command = new UpdateCartItemCommand(req);

        var existingItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Sku = "SKU-001",
            Quantity = 3, // Sẽ được update thành 10
            BasePrice = 25_000m
        };

        _cartItemRepositoryMock
            .Setup(x => x.GetByCustomerAndSkuAsync(customerId, req.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _cartItemRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Quantity.Should().Be(10);
        result.Sku.Should().Be(req.Sku);
        result.CustomerId.Should().Be(customerId);

        _cartItemRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<CartItem>(c => c.Quantity == 10), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // 2. THẤT BẠI - Item không tồn tại → throw Exception
    [Fact]
    public async Task Handle_ShouldThrowException_WhenCartItemNotFound()
    {
        // Arrange
        var req = new UpdateCartItemReq
        {
            CustomerId = Guid.NewGuid(),
            Sku = "SKU-NOT-EXIST",
            Quantity = 5
        };
        var command = new UpdateCartItemCommand(req);

        _cartItemRepositoryMock
            .Setup(x => x.GetByCustomerAndSkuAsync(req.CustomerId, req.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Cart item not found");

        // Xác nhận UpdateAsync không bị gọi khi bị lỗi
        _cartItemRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
