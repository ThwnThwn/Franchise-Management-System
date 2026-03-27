using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.WarehouseItems.Command.DeleteWarehouseItem;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Tests.Features.WarehouseItems;

public class DeleteWarehouseItemHandlerTests
{
    private readonly Mock<IWarehouseItemRepository> _itemRepositoryMock;
    private readonly DeleteWarehouseItemCommandHandler _handler;

    // Reusable item with zero quantity — safe to delete by default.
    private readonly Domain.Entities.WarehouseItem _existingItem;

    public DeleteWarehouseItemHandlerTests()
    {
        _itemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new DeleteWarehouseItemCommandHandler(_itemRepositoryMock.Object);

        _existingItem = new Domain.Entities.WarehouseItem
        {
            Id = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            IngredientId = Guid.NewGuid(),
            Unit = "kg",
            Quantity = 0,           // zero — safe to delete
            BasePrice = 10m,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Default: item found
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingItem);

        _itemRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — ITEM NOT FOUND
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenItemDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.WarehouseItem?)null);

        var command = new DeleteWarehouseItemCommand(id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenItemDoesNotExist()
    {
        // Arrange
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.WarehouseItem?)null);

        var command = new DeleteWarehouseItemCommand(Guid.NewGuid());

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _itemRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — STOCK REMAINING GUARD
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(0.01)]   // just above zero
    [InlineData(1)]      // typical stock
    [InlineData(999.99)] // large stock
    public async Task Handle_ShouldThrowInvalidOperationException_WhenItemHasStockRemaining(decimal quantity)
    {
        // Arrange
        _existingItem.Quantity = quantity;
        var command = new DeleteWarehouseItemCommand(_existingItem.Id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*stock remaining*");
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenQuantityIsExactlyZero()
    {
        // Arrange — boundary: Quantity == 0 is the only allowed value
        _existingItem.Quantity = 0;
        var command = new DeleteWarehouseItemCommand(_existingItem.Id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenItemHasStockRemaining()
    {
        // Arrange
        _existingItem.Quantity = 5;
        var command = new DeleteWarehouseItemCommand(_existingItem.Id);

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _itemRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — SUCCESSFUL SOFT DELETE
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithIsDeletedTrue_WhenRequestIsValid()
    {
        // Arrange
        var command = new DeleteWarehouseItemCommand(_existingItem.Id);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_existingItem.Id);
        result.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetIsDeletedTrue_OnItemEntity()
    {
        // Arrange
        var command = new DeleteWarehouseItemCommand(_existingItem.Id);

        // Act
        await _handler.Handle(command, default);

        // Assert — entity was mutated before SaveChanges
        _existingItem.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetUpdatedAt_ToApproximatelyNow_OnSoftDelete()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        var command = new DeleteWarehouseItemCommand(_existingItem.Id);

        // Act
        await _handler.Handle(command, default);

        // Assert
        _existingItem.UpdatedAt.Should().BeOnOrAfter(before);
        _existingItem.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce_WhenRequestIsValid()
    {
        // Arrange
        var command = new DeleteWarehouseItemCommand(_existingItem.Id);

        // Act
        await _handler.Handle(command, default);

        // Assert
        _itemRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdAsync_WithCorrectId()
    {
        // Arrange
        var command = new DeleteWarehouseItemCommand(_existingItem.Id);

        // Act
        await _handler.Handle(command, default);

        // Assert
        _itemRepositoryMock.Verify(
            x => x.GetByIdAsync(_existingItem.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}