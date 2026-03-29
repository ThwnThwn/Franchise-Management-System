using Moq;
using FluentAssertions;
using SalesService.Application.Dtos.CartItem.Request;
using SalesService.Application.Dtos.CartItem.Response;
using SalesService.Application.Features.CartItems.Commands.Create;
using SalesService.Application.Interfaces.CartItems;
using SalesService.Domain.Entities;

namespace SalesService.Application.Tests.Features.CartItems;

public class CreateCartItemCommandHandlerTests
{
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly CreateCartItemCommandHandler _handler;

    public CreateCartItemCommandHandlerTests()
    {
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _handler = new CreateCartItemCommandHandler(_cartItemRepositoryMock.Object);
    }

    // 1. THÊM MỚI - Item chưa có trong giỏ → tạo mới
    [Fact]
    public async Task Handle_ShouldCreateNewCartItem_WhenItemNotExist()
    {
        // Arrange
        var req = new CreateCartItemReq
        {
            CustomerId = Guid.NewGuid(),
            Sku = "SKU-001",
            Quantity = 2,
            BasePrice = 50_000m
        };
        var command = new CreateCartItemCommand(req);

        // Mock: không tìm thấy item cũ
        _cartItemRepositoryMock
            .Setup(x => x.GetByCustomerAndSkuAsync(req.CustomerId, req.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        _cartItemRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem c, CancellationToken _) => c);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Sku.Should().Be(req.Sku);
        result.Quantity.Should().Be(req.Quantity);
        result.BasePrice.Should().Be(req.BasePrice);
        result.CustomerId.Should().Be(req.CustomerId);

        // Xác nhận CreateAsync được gọi đúng 1 lần
        _cartItemRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<CartItem>(c => c.Sku == req.Sku && c.Quantity == req.Quantity),
                               It.IsAny<CancellationToken>()),
            Times.Once);

        // Xác nhận UpdateAsync KHÔNG được gọi
        _cartItemRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // 2. TĂNG SỐ LƯỢNG - Item đã tồn tại trong giỏ → cộng thêm quantity
    [Fact]
    public async Task Handle_ShouldIncreaseQuantity_WhenItemAlreadyExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var req = new CreateCartItemReq
        {
            CustomerId = customerId,
            Sku = "SKU-002",
            Quantity = 3,
            BasePrice = 30_000m
        };
        var command = new CreateCartItemCommand(req);

        var existingItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Sku = "SKU-002",
            Quantity = 2, // Đang có 2, thêm 3 → phải thành 5
            BasePrice = 30_000m,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Mock: tìm thấy item cũ
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
        result.Quantity.Should().Be(5); // 2 cũ + 3 mới
        result.Sku.Should().Be(req.Sku);

        // Xác nhận UpdateAsync được gọi, CreateAsync KHÔNG được gọi
        _cartItemRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<CartItem>(c => c.Quantity == 5), It.IsAny<CancellationToken>()),
            Times.Once);
        _cartItemRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
