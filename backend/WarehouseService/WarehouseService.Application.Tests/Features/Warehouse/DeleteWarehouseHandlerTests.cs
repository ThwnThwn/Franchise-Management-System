using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.Warehouses.Command.DeleteWarehouse;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;

namespace WarehouseService.Application.Tests.Features.Warehouses;

public class DeleteWarehouseHandlerTests
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly DeleteWarehouseCommandHandler _handler;

    // A reusable warehouse with NO active items (safe to delete by default).
    private readonly Domain.Entities.Warehouse _existingWarehouse;

    public DeleteWarehouseHandlerTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();

        _handler = new DeleteWarehouseCommandHandler(_repositoryMock.Object);

        _existingWarehouse = new Domain.Entities.Warehouse
        {
            Id = Guid.NewGuid(),
            Code = "WH-001",
            Name = "Test Warehouse",
            Address = "1 Test Street",
            StoreId = Guid.NewGuid(),
            IsDeleted = false,
            WarehouseItems = new List<WarehouseItem>(), // empty — no active items
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Default: warehouse found
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingWarehouse);

        // Default: queryable returns the warehouse with no active items
        SetupQueryable(_existingWarehouse);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — WAREHOUSE NOT FOUND
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenWarehouseDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Warehouse?)null);

        var command = new DeleteWarehouseCommand(id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenWarehouseDoesNotExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Warehouse?)null);

        var command = new DeleteWarehouseCommand(Guid.NewGuid());

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _repositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — ACTIVE ITEMS GUARD
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenWarehouseHasActiveItems()
    {
        // Arrange
        var warehouseWithItems = BuildWarehouse(items: new List<WarehouseItem>
        {
            new() { IsDeleted = false, Quantity = 5 }   // active item
        });

        _repositoryMock
            .Setup(x => x.GetByIdAsync(warehouseWithItems.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouseWithItems);

        SetupQueryable(warehouseWithItems);

        var command = new DeleteWarehouseCommand(warehouseWithItems.Id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*items in stock*");
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenAllItemsAreDeleted()
    {
        // Arrange — soft-deleted items should not block deletion
        var warehouseWithDeletedItems = BuildWarehouse(items: new List<WarehouseItem>
        {
            new() { IsDeleted = true, Quantity = 10 }   // soft-deleted, ignored
        });

        _repositoryMock
            .Setup(x => x.GetByIdAsync(warehouseWithDeletedItems.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouseWithDeletedItems);

        SetupQueryable(warehouseWithDeletedItems);

        var command = new DeleteWarehouseCommand(warehouseWithDeletedItems.Id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenItemsHaveZeroQuantity()
    {
        // Arrange — quantity = 0 is not "in stock"
        var warehouseWithZeroQty = BuildWarehouse(items: new List<WarehouseItem>
        {
            new() { IsDeleted = false, Quantity = 0 }
        });

        _repositoryMock
            .Setup(x => x.GetByIdAsync(warehouseWithZeroQty.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouseWithZeroQty);

        SetupQueryable(warehouseWithZeroQty);

        var command = new DeleteWarehouseCommand(warehouseWithZeroQty.Id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenAtLeastOneItemIsActiveAmongMany()
    {
        // Arrange — mixed items; one active is enough to block deletion
        var warehouse = BuildWarehouse(items: new List<WarehouseItem>
        {
            new() { IsDeleted = true,  Quantity = 10 },  // deleted  — ignored
            new() { IsDeleted = false, Quantity = 0  },  // zero qty — ignored
            new() { IsDeleted = false, Quantity = 1  }   // active   — blocks deletion
        });

        _repositoryMock
            .Setup(x => x.GetByIdAsync(warehouse.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        SetupQueryable(warehouse);

        var command = new DeleteWarehouseCommand(warehouse.Id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenWarehouseHasActiveItems()
    {
        // Arrange
        var warehouseWithItems = BuildWarehouse(items: new List<WarehouseItem>
        {
            new() { IsDeleted = false, Quantity = 3 }
        });

        _repositoryMock
            .Setup(x => x.GetByIdAsync(warehouseWithItems.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouseWithItems);

        SetupQueryable(warehouseWithItems);

        var command = new DeleteWarehouseCommand(warehouseWithItems.Id);

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _repositoryMock.Verify(
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
        var command = new DeleteWarehouseCommand(_existingWarehouse.Id);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_existingWarehouse.Id);
        result.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetIsDeletedTrue_OnWarehouseEntity()
    {
        // Arrange
        var command = new DeleteWarehouseCommand(_existingWarehouse.Id);

        // Act
        await _handler.Handle(command, default);

        // Assert — the entity itself was mutated before SaveChanges
        _existingWarehouse.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetUpdatedAt_ToApproximatelyNow_OnSoftDelete()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        var command = new DeleteWarehouseCommand(_existingWarehouse.Id);

        // Act
        await _handler.Handle(command, default);

        // Assert
        _existingWarehouse.UpdatedAt.Should().BeOnOrAfter(before);
        _existingWarehouse.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce_WhenRequestIsValid()
    {
        // Arrange
        var command = new DeleteWarehouseCommand(_existingWarehouse.Id);

        // Act
        await _handler.Handle(command, default);

        // Assert
        _repositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCallGetQueryable_ToCheckActiveItems()
    {
        // Arrange
        var command = new DeleteWarehouseCommand(_existingWarehouse.Id);

        // Act
        await _handler.Handle(command, default);

        // Assert
        _repositoryMock.Verify(
            x => x.GetQueryable(),
            Times.Once
        );
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    /// <summary>
    /// Wraps a single warehouse in a real in-memory list so IQueryable LINQ
    /// (SelectMany, Any, Where) executes correctly without a real EF DbSet.
    /// </summary>
    private void SetupQueryable(Domain.Entities.Warehouse warehouse)
    {
        var queryable = new List<Domain.Entities.Warehouse> { warehouse }.AsQueryable();
        _repositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(queryable);
    }

    private static Domain.Entities.Warehouse BuildWarehouse(List<WarehouseItem>? items = null) => new()
    {
        Id = Guid.NewGuid(),
        Code = "WH-TEST",
        Name = "Test Warehouse",
        Address = "1 Test Street",
        StoreId = Guid.NewGuid(),
        IsDeleted = false,
        WarehouseItems = items ?? new List<WarehouseItem>(),
        CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
        UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
    };
}