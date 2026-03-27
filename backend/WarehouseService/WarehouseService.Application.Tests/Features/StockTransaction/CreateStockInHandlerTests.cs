using Moq;
using Xunit;
using FluentAssertions;
using MockQueryable;
using WarehouseService.Application.Features.StockTransactions.Command.CreateStockIn;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using WarehouseItemEntity = WarehouseService.Domain.Entities.WarehouseItem;
using WarehouseEntity = WarehouseService.Domain.Entities.Warehouse;
using StockTransactionEntity = WarehouseService.Domain.Entities.StockTransaction;

namespace WarehouseService.Application.Tests.Features.StockTransactions;

public class CreateStockInHandlerTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _itemRepositoryMock;
    private readonly Mock<IStockTransactionRepository> _transactionRepositoryMock;
    private readonly CreateStockInCommandHandler _handler;

    // A default warehouse item used across tests.
    private readonly WarehouseItemEntity _defaultItem;

    public CreateStockInHandlerTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _itemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _transactionRepositoryMock = new Mock<IStockTransactionRepository>();

        _handler = new CreateStockInCommandHandler(
            _warehouseRepositoryMock.Object,
            _itemRepositoryMock.Object,
            _transactionRepositoryMock.Object
        );

        _defaultItem = new WarehouseItemEntity
        {
            Id = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),   // overridden per-test when needed
            IngredientId = Guid.NewGuid(),
            Unit = "kg",
            Quantity = 10m,
            BasePrice = 5m,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Default: warehouse exists
        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WarehouseEntity());

        // Default: AddAsync and SaveChangesAsync are no-ops
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

        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity>());

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[]
            {
                new StockInItemDto(ingredientId, 10, 5),
                new StockInItemDto(ingredientId, 20, 5)  // duplicate
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
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity>());

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[]
            {
                new StockInItemDto(ingredientId, 5, 2),
                new StockInItemDto(ingredientId, 5, 2)
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

        // Queryable returns no matching item
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity>());

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockInItemDto(ingredientId, 10, 5) });

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

        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity>());

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockInItemDto(ingredientId, 10, 5) });

        // Act & Assert — message contains BOTH IDs
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{warehouseId}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — SUCCESSFUL CREATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithCorrectSummaryFields_WhenRequestIsValid()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId);
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            supplierId: supplierId,
            referenceCode: "PO-2026-001",
            items: new[] { new StockInItemDto(ingredientId, 10, 25m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.WarehouseId.Should().Be(warehouseId);
        result.SupplierId.Should().Be(supplierId);
        result.ReferenceCode.Should().Be("PO-2026-001");
        result.TotalItems.Should().Be(1);
        result.TotalAmount.Should().Be(250m);  // 10 * 25
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldCreateTransactions_WithPendingStatus()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId);
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockInItemDto(ingredientId, 5, 10m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert — handler sets Status = Pending; quantity is NOT updated yet
        result.Transactions.Should().AllSatisfy(t =>
            t.Status.Should().Be(TransactionStatus.Pending));
    }

    [Fact]
    public async Task Handle_ShouldCreateTransactions_WithStockInType()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId);
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockInItemDto(ingredientId, 5, 10m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Transactions.Should().AllSatisfy(t =>
            t.Type.Should().Be(TransactionType.StockIn));
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalPrice_PerTransaction()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId);
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockInItemDto(ingredientId, Quantity: 7, UnitPrice: 15m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Transactions[0].TotalPrice.Should().Be(105m);  // 7 * 15
        result.TotalAmount.Should().Be(105m);
    }

    [Fact]
    public async Task Handle_ShouldGenerateTransactionCode_WithSIPrefix()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId);
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[] { new StockInItemDto(ingredientId, 1, 1m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Transactions[0].TransactionCode.Should().StartWith("SI-");
    }

    [Fact]
    public async Task Handle_ShouldMapIngredientId_ToEachTransactionDto()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId1 = Guid.NewGuid();
        var ingredientId2 = Guid.NewGuid();

        var item1 = BuildWarehouseItem(warehouseId, ingredientId1);
        var item2 = BuildWarehouseItem(warehouseId, ingredientId2);
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity> { item1, item2 });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[]
            {
                new StockInItemDto(ingredientId1, 5, 10m),
                new StockInItemDto(ingredientId2, 3, 20m)
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

    [Fact]
    public async Task Handle_ShouldSetSupplierId_ToNull_WhenNotProvided()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var item = BuildWarehouseItem(warehouseId, ingredientId);
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity> { item });

        var command = BuildCommand(
            warehouseId: warehouseId,
            supplierId: null,
            items: new[] { new StockInItemDto(ingredientId, 1, 1m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.SupplierId.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // GROUP 5 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallAddAsync_OncePerItem_WhenRequestIsValid()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId1 = Guid.NewGuid();
        var ingredientId2 = Guid.NewGuid();

        var item1 = BuildWarehouseItem(warehouseId, ingredientId1);
        var item2 = BuildWarehouseItem(warehouseId, ingredientId2);
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity> { item1, item2 });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[]
            {
                new StockInItemDto(ingredientId1, 5, 10m),
                new StockInItemDto(ingredientId2, 3, 15m)
            });

        // Act
        await _handler.Handle(command, default);

        // Assert — AddAsync called once per item, SaveChanges called once total
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
        var ingredientId1 = Guid.NewGuid();
        var ingredientId2 = Guid.NewGuid();

        var item1 = BuildWarehouseItem(warehouseId, ingredientId1);
        var item2 = BuildWarehouseItem(warehouseId, ingredientId2);
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity> { item1, item2 });

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: new[]
            {
                new StockInItemDto(ingredientId1, 5, 10m),
                new StockInItemDto(ingredientId2, 3, 15m)
            });

        // Act
        await _handler.Handle(command, default);

        // Assert — single round-trip regardless of item count
        _transactionRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 6 — EMPTY ITEMS LIST (foreach body never executes)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnEmptyTransactions_WhenItemsListIsEmpty()
    {
        // Arrange — empty Items list: both foreach loops are skipped entirely
        var warehouseId = Guid.NewGuid();
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity>());

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: Array.Empty<StockInItemDto>());

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
        var warehouseId = Guid.NewGuid();
        SetupItemQueryable(warehouseId, new List<WarehouseItemEntity>());

        var command = BuildCommand(
            warehouseId: warehouseId,
            items: Array.Empty<StockInItemDto>());

        // Act
        await _handler.Handle(command, default);

        // Assert — no transactions created, but SaveChanges is still called once
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
    // HELPERS
    // -----------------------------------------------------------------------

    /// <summary>
    /// Sets up GetQueryable() to return items matching the given warehouseId,
    /// using MockQueryable so FirstOrDefaultAsync works in-memory.
    /// </summary>
    private void SetupItemQueryable(Guid warehouseId, List<WarehouseItemEntity> items)
    {
        var mock = items.BuildMock<WarehouseItemEntity>();
        _itemRepositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(mock);
    }

    private static WarehouseItemEntity BuildWarehouseItem(Guid warehouseId, Guid ingredientId) => new()
    {
        Id = Guid.NewGuid(),
        WarehouseId = warehouseId,
        IngredientId = ingredientId,
        Unit = "kg",
        Quantity = 10m,
        BasePrice = 5m,
        IsDeleted = false,
        CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
        UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
    };

    private static CreateStockInCommand BuildCommand(
        Guid? warehouseId = null,
        IEnumerable<StockInItemDto>? items = null,
        Guid? supplierId = null,
        string? referenceCode = null,
        string? note = null,
        Guid? createdBy = null
    ) => new CreateStockInCommand(
        WarehouseId: warehouseId ?? Guid.NewGuid(),
        Items: (items ?? new[] { new StockInItemDto(Guid.NewGuid(), 1m, 10m) }).ToList(),
        SupplierId: supplierId,
        ReferenceCode: referenceCode,
        Note: note,
        CreatedBy: createdBy ?? Guid.NewGuid()
    );
}