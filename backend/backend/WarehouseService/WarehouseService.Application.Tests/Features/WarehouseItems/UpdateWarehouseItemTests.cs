namespace WarehouseService.Application.Tests.Features.WarehouseItems;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.WarehouseItems.Command.UpdateWarehouseItem;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;

public class UpdateWarehouseItemTests
{
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly UpdateWarehouseItemCommandHandler _handler;

    public UpdateWarehouseItemTests()
    {
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new UpdateWarehouseItemCommandHandler(_warehouseItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenItemDoesNotExist()
    {
        var itemId = Guid.NewGuid();
        var command = new UpdateWarehouseItemCommand(itemId, "Gói 1kg", 180000, null);

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseItem?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse item with ID '{itemId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateWarehouseItem_WhenRequestIsValid()
    {
        var itemId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // ID hạt cà phê Robusta Honey
        var expiryDate = DateTime.UtcNow.AddMonths(12);

        // Mock dữ liệu cũ: Hạt Robusta đóng lon 500g
        var existingItem = new WarehouseItem
        {
            Id = itemId,
            WarehouseId = warehouseId,
            IngredientId = ingredientId,
            Unit = "Lon 500g",
            BasePrice = 120000,
            Quantity = 50,
            ExpiryDate = DateTime.UtcNow.AddMonths(3),
            StockTransactions = []
        };

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        // Command mới: Cập nhật sang túi 1kg và giá mới
        var command = new UpdateWarehouseItemCommand(
            itemId,
            " Túi 1kg ", // Test logic Trim()
            230000,
            expiryDate);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().Be(itemId);
        result.Unit.Should().Be("Túi 1kg");
        result.BasePrice.Should().Be(230000);
        result.ExpiryDate.Should().Be(expiryDate);

        existingItem.Unit.Should().Be("Túi 1kg");
        existingItem.BasePrice.Should().Be(230000);

        _warehouseItemRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}