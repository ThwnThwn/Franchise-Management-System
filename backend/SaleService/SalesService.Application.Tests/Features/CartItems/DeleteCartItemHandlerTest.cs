using Moq;
using FluentAssertions;
using SalesService.Application.Dtos.CartItem.Request;
using SalesService.Application.Features.CartItems.Commands.Delete.DeleteAllCartItem;
using SalesService.Application.Features.CartItems.Commands.Delete.DeleteListCartItem;
using SalesService.Application.Interfaces.CartItems;

namespace SalesService.Application.Tests.Features.CartItems;

public class DeleteCartItemHandlerTests
{
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;

    public DeleteCartItemHandlerTests()
    {
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
    }

    // ===== DELETE ALL =====

    // 1. THÀNH CÔNG - Xóa toàn bộ giỏ hàng của customer
    [Fact]
    public async Task DeleteAll_Handle_ShouldClearCart_AndReturnTrue()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var req = new DeleteAllCartItemReq { CustomerId = customerId };
        var command = new DeleteAllCartItemCommand(req);
        var handler = new DeleteAllCartItemCommandHandler(_cartItemRepositoryMock.Object);

        _cartItemRepositoryMock
            .Setup(x => x.ClearCartAsync(customerId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _cartItemRepositoryMock.Verify(
            x => x.ClearCartAsync(customerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ===== DELETE LIST (theo SKU) =====

    // 2. THÀNH CÔNG - Xóa danh sách SKU cụ thể
    [Fact]
    public async Task DeleteList_Handle_ShouldRemoveItems_AndReturnTrue()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var skus = new List<string> { "SKU-001", "SKU-002" };
        var req = new DeleteListCartItemReq { CustomerId = customerId, Skus = skus };
        var command = new DeleteListCartItemCommand(req);
        var handler = new DeleteListCartItemCommandHandler(_cartItemRepositoryMock.Object);

        _cartItemRepositoryMock
            .Setup(x => x.RemoveItemsAsync(customerId, skus, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _cartItemRepositoryMock.Verify(
            x => x.RemoveItemsAsync(customerId, skus, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
