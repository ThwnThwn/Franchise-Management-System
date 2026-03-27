using Moq;
using FluentAssertions;
using MockQueryable;
using WarehouseService.Application.Features.StockTransactions.Command.CreateStockOut;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using WarehouseItemEntity = WarehouseService.Domain.Entities.WarehouseItem;
using WarehouseEntity = WarehouseService.Domain.Entities.Warehouse;
using StockTransactionEntity = WarehouseService.Domain.Entities.StockTransaction;

namespace WarehouseService.Application.Tests.Features.StockTransactions;

public class CreateStockOutHandlerTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _itemRepositoryMock;
    private readonly Mock<IStockTransactionRepository> _transactionRepositoryMock;
    private readonly CreateStockOutCommandHandler _handler;

    public CreateStockOutHandlerTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _itemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _transactionRepositoryMock = new Mock<IStockTransactionRepository>();

        _handler = new CreateStockOutCommandHandler(
            _warehouseRepositoryMock.Object,
            _itemRepositoryMock.Object,
            _transactionRepositoryMock.Object
        );

        // Default: warehouse exists, no-op persistence
        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WarehouseEntity());

        _transactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<StockTransactionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transactionRepositoryMock
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
        var warehouseId = Guid.NewGuid();

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseEntity?)null);

        var command = BuildCommand(warehouseId: warehouseId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{warehouseId}*");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenWarehouseDoesNotExist()
    {
        // Arrange
        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseEntity?)null);

        var command = BuildCommand();

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _transactionRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — DUPLICATE INGREDIENT IN REQUEST
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenRequestContainsDuplicateIngredientId()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        SetupItemQueryable(new List<WarehouseItemEntity>());

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[]
            {
                new StockOutItemDto(ingredientId, 5, 10m),
                new StockOutItemDto(ingredientId, 3, 10m)  // duplicate
            });

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{ingredientId}*");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenDuplicateIngredientDetected()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        SetupItemQueryable(new List<WarehouseItemEntity>());

        var command = BuildCommand(items: new[]
        {
            new StockOutItemDto(ingredientId, 5, 10m),
            new StockOutItemDto(ingredientId, 3, 10m)
        });

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _transactionRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — INGREDIENT NOT IN WAREHOUSE
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIngredientNotInWarehouse()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        SetupItemQueryable(new List<WarehouseItemEntity>());  // no matching item

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, 5, 10m) });

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{ingredientId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_ContainingWarehouseId_WhenIngredientNotInWarehouse()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        SetupItemQueryable(new List<WarehouseItemEntity>());

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, 5, 10m) });

        // Act & Assert — message contains warehouseId too
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{warehouseId}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — INSUFFICIENT STOCK GUARD
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenRequestedQuantityExceedsStock()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        // item has 5 in stock, request wants 10
        var item = BuildWarehouseItem(warehouseId, ingredientId, quantity: 5m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, Quantity: 10m, UnitPrice: 5m) });

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{ingredientId}*");
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenRequestedQuantityEqualsStock()
    {
        // Arrange — exact boundary: requested == available
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId, quantity: 10m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, Quantity: 10m, UnitPrice: 5m) });

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenRequestedQuantityIsLessThanStock()
    {
        // Arrange — normal case: requested < available
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId, quantity: 20m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, Quantity: 10m, UnitPrice: 5m) });

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenStockIsInsufficient()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId, quantity: 3m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, Quantity: 10m, UnitPrice: 5m) });

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _transactionRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 5 — SUCCESSFUL CREATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithCorrectSummaryFields_WhenRequestIsValid()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId, quantity: 50m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, Quantity: 10m, UnitPrice: 20m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.WarehouseId.Should().Be(warehouseId);
        result.TotalItems.Should().Be(1);
        result.TotalAmount.Should().Be(200m);   // 10 * 20
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldCreateTransactions_WithPendingStatus()
    {
        // Arrange — quantity NOT deducted yet at creation time
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId, quantity: 50m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, 5m, 10m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Transactions.Should().AllSatisfy(t =>
            t.Status.Should().Be(TransactionStatus.Pending));
    }

    [Fact]
    public async Task Handle_ShouldCreateTransactions_WithStockOutType()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId, quantity: 50m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, 5m, 10m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Transactions.Should().AllSatisfy(t =>
            t.Type.Should().Be(TransactionType.StockOut));
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalPrice_PerTransaction()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId, quantity: 50m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, Quantity: 4m, UnitPrice: 25m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Transactions[0].TotalPrice.Should().Be(100m);  // 4 * 25
        result.TotalAmount.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_ShouldGenerateTransactionCode_WithSOPrefix()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId, quantity: 50m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, 1m, 1m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert — StockOut uses "SO-" prefix, not "SI-"
        result.Transactions[0].TransactionCode.Should().StartWith("SO-");
    }

    [Fact]
    public async Task Handle_ShouldMapIngredientId_ToEachTransactionDto()
    {
        // Arrange — two items, verify each DTO gets the right IngredientId
        var warehouseId = Guid.NewGuid();
        var ingredientId1 = Guid.NewGuid();
        var ingredientId2 = Guid.NewGuid();

        var item1 = BuildWarehouseItem(warehouseId, ingredientId1, quantity: 20m);
        var item2 = BuildWarehouseItem(warehouseId, ingredientId2, quantity: 30m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item1, item2 });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[]
            {
                new StockOutItemDto(ingredientId1, 5m, 10m),
                new StockOutItemDto(ingredientId2, 3m, 20m)
            });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Transactions.Should().HaveCount(2);
        result.Transactions.Select(t => t.IngredientId)
            .Should().BeEquivalentTo(new[] { ingredientId1, ingredientId2 });
        result.TotalItems.Should().Be(2);
        result.TotalAmount.Should().Be(110m);  // (5*10) + (3*20)
    }

    // -----------------------------------------------------------------------
    // GROUP 6 — EMPTY ITEMS LIST (foreach body never executes)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnEmptyTransactions_WhenItemsListIsEmpty()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        SetupItemQueryable(new List<WarehouseItemEntity>());

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: Array.Empty<StockOutItemDto>());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Transactions.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
        result.TotalAmount.Should().Be(0);
        result.WarehouseId.Should().Be(warehouseId);
    }

    [Fact]
    public async Task Handle_ShouldNotCallAddAsync_WhenItemsListIsEmpty()
    {
        // Arrange
        SetupItemQueryable(new List<WarehouseItemEntity>());

        var command = BuildCommand(items: Array.Empty<StockOutItemDto>());

        // Act
        await _handler.Handle(command, default);

        // Assert — no transactions created, but SaveChanges still fires once
        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<StockTransactionEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _transactionRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 7 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallAddAsync_OncePerItem_WhenRequestIsValid()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId1 = Guid.NewGuid();
        var ingredientId2 = Guid.NewGuid();

        var item1 = BuildWarehouseItem(warehouseId, ingredientId1, quantity: 20m);
        var item2 = BuildWarehouseItem(warehouseId, ingredientId2, quantity: 20m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item1, item2 });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[]
            {
                new StockOutItemDto(ingredientId1, 5m, 10m),
                new StockOutItemDto(ingredientId2, 3m, 15m)
            });

        // Act
        await _handler.Handle(command, default);

        // Assert — one AddAsync per item
        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<StockTransactionEntity>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce_WhenRequestIsValid()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId, quantity: 50m);
        SetupItemQueryable(new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockOutItemDto(ingredientId, 5m, 10m) });

        // Act
        await _handler.Handle(command, default);

        // Assert — single round-trip regardless of item count
        _transactionRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    private void SetupItemQueryable(List<WarehouseItemEntity> items)
    {
        var mock = items.BuildMock<WarehouseItemEntity>();
        _itemRepositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(mock);
    }

    private static WarehouseItemEntity BuildWarehouseItem(
        Guid warehouseId,
        Guid ingredientId,
        decimal quantity = 20m
    ) => new()
    {
        Id = Guid.NewGuid(),
        WarehouseId = warehouseId,
        IngredientId = ingredientId,
        Unit = "kg",
        Quantity = quantity,
        BasePrice = 5m,
        IsDeleted = false,
        CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
        UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
    };

    private static CreateStockOutCommand BuildCommand(
        Guid? warehouseId = null,
        IEnumerable<StockOutItemDto>? items = null,
        string? note = null,
        Guid? createdBy = null
    ) => new CreateStockOutCommand(
        WarehouseId: warehouseId ?? Guid.NewGuid(),
        Items: (items ?? new[] { new StockOutItemDto(Guid.NewGuid(), 1m, 10m) }).ToList(),
        Note: note,
        CreatedBy: createdBy ?? Guid.NewGuid()
    );
}