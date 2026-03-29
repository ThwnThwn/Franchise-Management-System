namespace WarehouseService.Application.Tests.Features.WarehouseItems;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.WarehouseItems.Command.DeleteWarehouseItem;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;

public class DeleteWarehouseItemTests
{
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly DeleteWarehouseItemCommandHandler _handler;

    public DeleteWarehouseItemTests()
    {
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new DeleteWarehouseItemCommandHandler(_warehouseItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenItemDoesNotExist()
    {
        var itemId = Guid.NewGuid();
        var command = new DeleteWarehouseItemCommand(itemId);

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseItem?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse item with ID '{itemId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenItemStillHasStock()
    {
        var itemId = Guid.NewGuid();
        var command = new DeleteWarehouseItemCommand(itemId);

        // Giả lập túi bột Matcha vẫn còn 1.5kg trong kho
        var matchaItem = new WarehouseItem
        {
            Id = itemId,
            Quantity = 1.5m,
            Unit = "Kg",
            StockTransactions = []
        };

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchaItem);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Warehouse item cannot be deleted because it still has stock remaining.");
    }

    [Fact]
    public async Task Handle_ShouldPerformSoftDelete_WhenQuantityIsZero()
    {
        var itemId = Guid.NewGuid();
        var command = new DeleteWarehouseItemCommand(itemId);

        // Giả lập siro đường đã hết sạch (Quantity = 0)
        var syrupItem = new WarehouseItem
        {
            Id = itemId,
            Quantity = 0,
            Unit = "Chai",
            IsDeleted = false,
            StockTransactions = []
        };

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(syrupItem);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().Be(itemId);
        result.IsDeleted.Should().BeTrue();
        syrupItem.IsDeleted.Should().BeTrue();

        _warehouseItemRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}