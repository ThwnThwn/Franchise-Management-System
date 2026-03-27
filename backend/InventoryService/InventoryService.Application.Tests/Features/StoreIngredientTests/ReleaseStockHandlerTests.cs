using FluentAssertions;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Features.StoreIngredient.Commands.ReleaseStock;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using Moq;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class ReleaseStockHandlerTests
{
    private readonly Mock<IStoreIngredientRepository> _storeRepoMock;
    private readonly ReleaseStockHandler _handler;

    public ReleaseStockHandlerTests()
    {
        _storeRepoMock = new Mock<IStoreIngredientRepository>();
        _handler = new ReleaseStockHandler(_storeRepoMock.Object);
    }

    // 1. Phủ nhánh: Không tìm thấy Queue (Dòng 31-39)
    // Test cả 2 trường hợp: null và danh sách rỗng để phủ toán tử ||
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldReturnFailure_WhenQueueNotFound(bool isNull)
    {
        // Arrange
        var req = new ReleaseStockReq { OrderId = Guid.NewGuid() };
        var cmd = new ReleaseStockCommand(req);

        _storeRepoMock.Setup(x => x.GetQueueByOrderIdAsync(req.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(isNull ? null! : new List<IngredientQueue>());

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("No reserved stock found");
        _storeRepoMock.Verify(x => x.RemoveQueueByOrderIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    // 2. Happy Path: Giải phóng giữ chỗ thành công (Dòng 45-51)
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenQueueExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var req = new ReleaseStockReq { OrderId = orderId };
        var cmd = new ReleaseStockCommand(req);

        // Giả lập có ít nhất 1 item trong queue
        _storeRepoMock.Setup(x => x.GetQueueByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<IngredientQueue> { new() });

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Stock released successfully");

        // Xác nhận hàm xóa được gọi đúng ID
        _storeRepoMock.Verify(x => x.RemoveQueueByOrderIdAsync(orderId), Times.Once);
    }
}