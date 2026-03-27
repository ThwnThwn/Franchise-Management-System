using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.WarehouseItems.Command.UpdateWarehouseItem;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Tests.Features.WarehouseItems;

public class UpdateWarehouseItemHandlerTests
{
    private readonly Mock<IWarehouseItemRepository> _itemRepositoryMock;
    private readonly UpdateWarehouseItemCommandHandler _handler;

    // Reusable existing item returned by GetByIdAsync in the happy path.
    private readonly Domain.Entities.WarehouseItem _existingItem;

    public UpdateWarehouseItemHandlerTests()
    {
        _itemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new UpdateWarehouseItemCommandHandler(_itemRepositoryMock.Object);

        _existingItem = new Domain.Entities.WarehouseItem
        {
            Id = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            IngredientId = Guid.NewGuid(),
            Unit = "kg",
            Quantity = 5m,
            BasePrice = 10m,
            ExpiryDate = null,
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

        var command = BuildCommand(id: id);

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

        var command = BuildCommand();

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
    // GROUP 2 — SUCCESSFUL UPDATE & FIELD MAPPING
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithAllFieldsMapped_WhenRequestIsValid()
    {
        // Arrange
        var expiry = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var command = BuildCommand(unit: "litre", basePrice: 25.50m, expiryDate: expiry);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_existingItem.Id);
        result.WarehouseId.Should().Be(_existingItem.WarehouseId);
        result.IngredientID.Should().Be(_existingItem.IngredientId);
        result.Unit.Should().Be("litre");
        result.Quantity.Should().Be(_existingItem.Quantity);   // unchanged
        result.BasePrice.Should().Be(25.50m);
        result.ExpiryDate.Should().Be(expiry);
        result.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldTrimWhitespace_FromUnit()
    {
        // Arrange
        var command = BuildCommand(unit: "  litre  ");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Unit.Should().Be("litre");
    }

    [Fact]
    public async Task Handle_ShouldUpdateExpiryDate_WhenExpiryDateIsProvided()
    {
        // Arrange
        var expiry = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var command = BuildCommand(expiryDate: expiry);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.ExpiryDate.Should().Be(expiry);
    }

    [Fact]
    public async Task Handle_ShouldSetExpiryDateToNull_WhenExpiryDateIsNull()
    {
        // Arrange — clear a previously set expiry date
        _existingItem.ExpiryDate = new DateTime(2025, 1, 1);
        var command = BuildCommand(expiryDate: null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.ExpiryDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldMutateEntityFields_BeforeSaving()
    {
        // Arrange
        var expiry = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var command = BuildCommand(unit: "pcs", basePrice: 5m, expiryDate: expiry);

        // Act
        await _handler.Handle(command, default);

        // Assert — entity was mutated in place
        _existingItem.Unit.Should().Be("pcs");
        _existingItem.BasePrice.Should().Be(5m);
        _existingItem.ExpiryDate.Should().Be(expiry);
    }

    [Fact]
    public async Task Handle_ShouldSetUpdatedAt_ToApproximatelyNow()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        var command = BuildCommand();

        // Act
        await _handler.Handle(command, default);

        // Assert
        _existingItem.UpdatedAt.Should().BeOnOrAfter(before);
        _existingItem.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldNotModifyQuantity_WhenUpdating()
    {
        // Arrange — Quantity is not part of the update command; must stay untouched
        var originalQuantity = _existingItem.Quantity;
        var command = BuildCommand();

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Quantity.Should().Be(originalQuantity);
        _existingItem.Quantity.Should().Be(originalQuantity);
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce_WhenRequestIsValid()
    {
        // Arrange
        var command = BuildCommand();

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
        var command = BuildCommand(id: _existingItem.Id);

        // Act
        await _handler.Handle(command, default);

        // Assert
        _itemRepositoryMock.Verify(
            x => x.GetByIdAsync(_existingItem.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    private static UpdateWarehouseItemCommand BuildCommand(
        Guid? id = null,
        string? unit = null,
        decimal basePrice = 20m,
        DateTime? expiryDate = null
    ) => new UpdateWarehouseItemCommand(
        Id: id ?? Guid.NewGuid(),
        Unit: unit ?? "kg",
        BasePrice: basePrice,
        ExpiryDate: expiryDate
    );
}