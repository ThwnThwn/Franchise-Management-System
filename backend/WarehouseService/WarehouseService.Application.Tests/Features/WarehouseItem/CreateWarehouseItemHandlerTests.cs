using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.WarehouseItems.Command.CreateWarehouseItem;
using WarehouseService.Application.Repositories;
using WarehouseService.Application.ClientInterfaces;

namespace WarehouseService.Application.Tests.Features.WarehouseItems;

public class CreateWarehouseItemHandlerTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _itemRepositoryMock;
    private readonly Mock<IInventoryServiceClient> _inventoryClientMock;
    private readonly CreateWarehouseItemCommandHandler _handler;

    public CreateWarehouseItemHandlerTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _itemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _inventoryClientMock = new Mock<IInventoryServiceClient>();

        _handler = new CreateWarehouseItemCommandHandler(
            _warehouseRepositoryMock.Object,
            _itemRepositoryMock.Object,
            _inventoryClientMock.Object
        );

        // Default happy-path: warehouse exists, ingredient exists, no duplicate
        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Warehouse());

        _inventoryClientMock
            .Setup(x => x.IngredientExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        _itemRepositoryMock
            .Setup(x => x.IngredientExistsInWarehouseAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _itemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.WarehouseItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _itemRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — WAREHOUSE / INGREDIENT NOT FOUND
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenWarehouseDoesNotExist()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Warehouse?)null);

        var command = BuildCommand(warehouseId: warehouseId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{warehouseId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIngredientDoesNotExist()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();

        _inventoryClientMock
            .Setup(x => x.IngredientExistsAsync(ingredientId))
            .ReturnsAsync(false);

        var command = BuildCommand(ingredientId: ingredientId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{ingredientId}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — DUPLICATE INGREDIENT IN WAREHOUSE
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenIngredientAlreadyExistsInWarehouse()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.IngredientExistsInWarehouseAsync(
                It.IsAny<Guid>(), ingredientId, It.IsAny<Guid>(),It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(ingredientId: ingredientId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{ingredientId}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — SUCCESSFUL CREATION & FIELD MAPPING
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithAllFieldsMapped_WhenRequestIsValid()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var expiry = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var command = BuildCommand(
            warehouseId: warehouseId,
            ingredientId: ingredientId,
            unit: "kg",
            quantity: 10.5m,
            basePrice: 99.99m,
            expiryDate: expiry
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.WarehouseId.Should().Be(warehouseId);
        result.IngredientId.Should().Be(ingredientId);
        result.Unit.Should().Be("kg");
        result.Quantity.Should().Be(10.5m);
        result.BasePrice.Should().Be(99.99m);
        result.ExpiryDate.Should().Be(expiry);
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldTrimWhitespace_FromUnit()
    {
        // Arrange
        var command = BuildCommand(unit: "  kg  ");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Unit.Should().Be("kg");
    }

    [Fact]
    public async Task Handle_ShouldReturnNullExpiryDate_WhenExpiryDateIsNotProvided()
    {
        // Arrange
        var command = BuildCommand(expiryDate: null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.ExpiryDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldSpecifyExpiryDateAsUtc_WhenExpiryDateIsProvided()
    {
        // Arrange — pass an Unspecified kind; handler must convert to UTC
        var rawDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var command = BuildCommand(expiryDate: rawDate);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.ExpiryDate.Should().NotBeNull();
        result.ExpiryDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — VALIDATION ORDER
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCheckWarehouseFirst_BeforeCheckingIngredient()
    {
        // Arrange — warehouse missing AND ingredient missing; only warehouse error fires
        var warehouseId = Guid.NewGuid();

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Warehouse?)null);

        _inventoryClientMock
            .Setup(x => x.IngredientExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(false);

        var command = BuildCommand(warehouseId: warehouseId);

        // Act & Assert — KeyNotFoundException from warehouse guard, not ingredient guard
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{warehouseId}*");
    }

    [Fact]
    public async Task Handle_ShouldCheckIngredientExistence_BeforeCheckingDuplicate()
    {
        // Arrange — ingredient missing AND duplicate conflict; only ingredient error fires
        var ingredientId = Guid.NewGuid();

        _inventoryClientMock
            .Setup(x => x.IngredientExistsAsync(ingredientId))
            .ReturnsAsync(false);

        _itemRepositoryMock
            .Setup(x => x.IngredientExistsInWarehouseAsync(
                It.IsAny<Guid>(), ingredientId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(ingredientId: ingredientId);

        // Act & Assert — KeyNotFoundException from ingredient guard, not duplicate guard
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{ingredientId}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 5 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallAddAsync_ExactlyOnce_WhenRequestIsValid()
    {
        // Arrange
        var command = BuildCommand();

        // Act
        await _handler.Handle(command, default);

        // Assert
        _itemRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.WarehouseItem>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

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
    public async Task Handle_ShouldNotCallAddAsync_WhenWarehouseDoesNotExist()
    {
        // Arrange
        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Warehouse?)null);

        var command = BuildCommand();

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _itemRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.WarehouseItem>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldNotCallAddAsync_WhenIngredientAlreadyExistsInWarehouse()
    {
        // Arrange
        _itemRepositoryMock
            .Setup(x => x.IngredientExistsInWarehouseAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand();

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _itemRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.WarehouseItem>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    private static CreateWarehouseItemCommand BuildCommand(
        Guid? warehouseId = null,
        Guid? ingredientId = null,
        string? unit = null,
        decimal quantity = 1m,
        decimal basePrice = 10m,
        DateTime? expiryDate = null
    ) => new CreateWarehouseItemCommand(
        WarehouseId: warehouseId ?? Guid.NewGuid(),
        IngredientId: ingredientId ?? Guid.NewGuid(),
        Unit: unit ?? "kg",
        Quantity: quantity,
        BasePrice: basePrice,
        ExpiryDate: expiryDate
    );
}