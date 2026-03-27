using Moq;
using Xunit;
using FluentAssertions;
using MockQueryable;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CompleteWarehouseTransfer;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using WarehouseTransferEntity = WarehouseService.Domain.Entities.WarehouseTransfer;
using WarehouseTransferItemEntity = WarehouseService.Domain.Entities.WarehouseTransferItem;
using WarehouseItemEntity = WarehouseService.Domain.Entities.WarehouseItem;
using StockTransactionEntity = WarehouseService.Domain.Entities.StockTransaction;

namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

public class CompleteWarehouseTransferHandlerTests
{
    private readonly Mock<IWarehouseTransferRepository> _transferRepositoryMock;
    private readonly Mock<IWarehouseTransferItemRepository> _transferItemRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly Mock<IStockTransactionRepository> _stockTransactionRepositoryMock;
    private readonly CompleteWarehouseTransferCommandHandler _handler;

    // Fixed IDs reused across tests
    private readonly Guid _transferId = Guid.NewGuid();
    private readonly Guid _fromWarehouseId = Guid.NewGuid();
    private readonly Guid _toWarehouseId = Guid.NewGuid();

    // Reusable approved transfer
    private readonly WarehouseTransferEntity _approvedTransfer;

    public CompleteWarehouseTransferHandlerTests()
    {
        _transferRepositoryMock = new Mock<IWarehouseTransferRepository>();
        _transferItemRepositoryMock = new Mock<IWarehouseTransferItemRepository>();
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _stockTransactionRepositoryMock = new Mock<IStockTransactionRepository>();

        _handler = new CompleteWarehouseTransferCommandHandler(
            _transferRepositoryMock.Object,
            _transferItemRepositoryMock.Object,
            _warehouseItemRepositoryMock.Object,
            _stockTransactionRepositoryMock.Object
        );

        _approvedTransfer = new WarehouseTransferEntity
        {
            Id = _transferId,
            TransferCode = "TRF-20260101-ABCDEF",
            Status = TransferStatus.Approved,
            FromWarehouseId = _fromWarehouseId,
            ToWarehouseId = _toWarehouseId,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Default: transfer found
        _transferRepositoryMock
            .Setup(x => x.GetByIdAsync(_transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_approvedTransfer);

        // Default: no-op persistence
        _stockTransactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<StockTransactionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _stockTransactionRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — TRANSFER HEADER VALIDATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransferDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _transferRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseTransferEntity?)null);

        var command = new CompleteWarehouseTransferCommand(id, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Theory]
    [InlineData(TransferStatus.Pending)]
    [InlineData(TransferStatus.Cancelled)]
    [InlineData(TransferStatus.Rejected)]
    [InlineData(TransferStatus.Completed)]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenTransferIsNotApproved(TransferStatus nonApprovedStatus)
    {
        // Arrange
        _approvedTransfer.Status = nonApprovedStatus;
        SetupTransferItems(new List<WarehouseTransferItemEntity>());

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{nonApprovedStatus}*");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenTransferDoesNotExist()
    {
        // Arrange
        _transferRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseTransferEntity?)null);

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _stockTransactionRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — TRANSFER ITEMS GUARD (no items = cannot complete)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenTransferHasNoItems()
    {
        // Arrange — approved transfer but zero transfer items
        SetupTransferItems(new List<WarehouseTransferItemEntity>());

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{_transferId}*");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenTransferHasNoItems()
    {
        // Arrange
        SetupTransferItems(new List<WarehouseTransferItemEntity>());

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _stockTransactionRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — PER-ITEM GUARDS (inside foreach)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenSourceItemDoesNotExist()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var transferItemId = Guid.NewGuid();

        SetupTransferItems(new List<WarehouseTransferItemEntity>
        {
            BuildTransferItem(transferItemId, ingredientId, quantity: 5m)
        });

        // Source item not found
        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(transferItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseItemEntity?)null);

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{transferItemId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenSourceItemHasInsufficientStock()
    {
        // Arrange — source has 3, transfer wants 10
        var ingredientId = Guid.NewGuid();
        var transferItemId = Guid.NewGuid();

        SetupTransferItems(new List<WarehouseTransferItemEntity>
        {
            BuildTransferItem(transferItemId, ingredientId, quantity: 10m)
        });

        var fromItem = BuildWarehouseItem(transferItemId, _fromWarehouseId, ingredientId, quantity: 3m);
        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(transferItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromItem);

        SetupWarehouseItemQueryable(new List<WarehouseItemEntity>());

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{ingredientId}*");
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenSourceItemQuantityEqualsRequestedQuantity()
    {
        // Arrange — exact boundary: available == requested
        var ingredientId = Guid.NewGuid();
        var transferItemId = Guid.NewGuid();

        SetupTransferItems(new List<WarehouseTransferItemEntity>
        {
            BuildTransferItem(transferItemId, ingredientId, quantity: 10m)
        });

        var fromItem = BuildWarehouseItem(transferItemId, _fromWarehouseId, ingredientId, quantity: 10m);
        var toItem = BuildWarehouseItem(Guid.NewGuid(), _toWarehouseId, ingredientId, quantity: 5m);

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(transferItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromItem);

        SetupWarehouseItemQueryable(new List<WarehouseItemEntity> { toItem });

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenDestinationItemDoesNotExist()
    {
        // Arrange — source exists and has stock, but destination has no matching ingredient
        var ingredientId = Guid.NewGuid();
        var transferItemId = Guid.NewGuid();

        SetupTransferItems(new List<WarehouseTransferItemEntity>
        {
            BuildTransferItem(transferItemId, ingredientId, quantity: 5m)
        });

        var fromItem = BuildWarehouseItem(transferItemId, _fromWarehouseId, ingredientId, quantity: 20m);
        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(transferItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromItem);

        // No matching destination item
        SetupWarehouseItemQueryable(new List<WarehouseItemEntity>());

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{ingredientId}*");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenSourceItemHasInsufficientStock()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var transferItemId = Guid.NewGuid();

        SetupTransferItems(new List<WarehouseTransferItemEntity>
        {
            BuildTransferItem(transferItemId, ingredientId, quantity: 10m)
        });

        var fromItem = BuildWarehouseItem(transferItemId, _fromWarehouseId, ingredientId, quantity: 1m);
        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(transferItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromItem);

        SetupWarehouseItemQueryable(new List<WarehouseItemEntity>());

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _stockTransactionRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — SUCCESSFUL COMPLETION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithCompletedStatus_WhenRequestIsValid()
    {
        // Arrange
        var (fromItem, toItem, transferItems) = SetupValidSingleItemTransfer();
        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_transferId);
        result.TransferCode.Should().Be("TRF-20260101-ABCDEF");
        result.Status.Should().Be(TransferStatus.Completed);
        result.FromWarehouseId.Should().Be(_fromWarehouseId);
        result.ToWarehouseId.Should().Be(_toWarehouseId);
        result.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldDeductQuantityFromSource_AndAddToDestination()
    {
        // Arrange
        var (fromItem, toItem, _) = SetupValidSingleItemTransfer(
            fromQty: 20m, toQty: 5m, transferQty: 8m);

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        await _handler.Handle(command, default);

        // Assert — stock mutated correctly
        fromItem.Quantity.Should().Be(12m);   // 20 - 8
        toItem.Quantity.Should().Be(13m);     // 5 + 8
    }

    [Fact]
    public async Task Handle_ShouldMarkTransferAsCompleted_OnEntity()
    {
        // Arrange
        SetupValidSingleItemTransfer();
        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        await _handler.Handle(command, default);

        // Assert — entity mutated in place
        _approvedTransfer.Status.Should().Be(TransferStatus.Completed);
        _approvedTransfer.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectTransferredItemInfo()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var transferItemId = Guid.NewGuid();

        SetupTransferItems(new List<WarehouseTransferItemEntity>
        {
            BuildTransferItem(transferItemId, ingredientId, quantity: 7m)
        });

        var fromItem = BuildWarehouseItem(transferItemId, _fromWarehouseId, ingredientId, quantity: 20m, unit: "litre");
        var toItem = BuildWarehouseItem(Guid.NewGuid(), _toWarehouseId, ingredientId, quantity: 5m);

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(transferItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromItem);

        SetupWarehouseItemQueryable(new List<WarehouseItemEntity> { toItem });

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.TransferredItems.Should().HaveCount(1);
        var info = result.TransferredItems[0];
        info.IngredientId.Should().Be(ingredientId);
        info.Unit.Should().Be("litre");
        info.Quantity.Should().Be(7m);
        info.FromWarehouseItemId.Should().Be(fromItem.Id);
        info.ToWarehouseItemId.Should().Be(toItem.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnMultipleTransferredItems_WhenTransferHasMultipleItems()
    {
        // Arrange
        var ingredientId1 = Guid.NewGuid();
        var ingredientId2 = Guid.NewGuid();
        var transferItemId1 = Guid.NewGuid();
        var transferItemId2 = Guid.NewGuid();

        SetupTransferItems(new List<WarehouseTransferItemEntity>
        {
            BuildTransferItem(transferItemId1, ingredientId1, quantity: 5m),
            BuildTransferItem(transferItemId2, ingredientId2, quantity: 3m)
        });

        var fromItem1 = BuildWarehouseItem(transferItemId1, _fromWarehouseId, ingredientId1, quantity: 20m);
        var fromItem2 = BuildWarehouseItem(transferItemId2, _fromWarehouseId, ingredientId2, quantity: 15m);
        var toItem1 = BuildWarehouseItem(Guid.NewGuid(), _toWarehouseId, ingredientId1, quantity: 2m);
        var toItem2 = BuildWarehouseItem(Guid.NewGuid(), _toWarehouseId, ingredientId2, quantity: 4m);

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(transferItemId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromItem1);

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(transferItemId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromItem2);

        SetupWarehouseItemQueryable(new List<WarehouseItemEntity> { toItem1, toItem2 });

        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.TransferredItems.Should().HaveCount(2);
    }

    // -----------------------------------------------------------------------
    // GROUP 5 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallAddAsync_TwicePerItem_ForTransferOutAndTransferIn()
    {
        // Arrange — 1 item → 2 transactions (TransferOut + TransferIn)
        SetupValidSingleItemTransfer();
        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        await _handler.Handle(command, default);

        // Assert
        _stockTransactionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<StockTransactionEntity>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateTransferOutTransaction_WithCorrectType()
    {
        // Arrange
        var capturedTransactions = new List<StockTransactionEntity>();

        _stockTransactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<StockTransactionEntity>(), It.IsAny<CancellationToken>()))
            .Callback<StockTransactionEntity, CancellationToken>((t, _) => capturedTransactions.Add(t))
            .Returns(Task.CompletedTask);

        SetupValidSingleItemTransfer();
        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        await _handler.Handle(command, default);

        // Assert
        capturedTransactions.Should().Contain(t => t.Type == TransactionType.TransferOut);
        capturedTransactions.Should().Contain(t => t.Type == TransactionType.TransferIn);
        capturedTransactions.Should().AllSatisfy(t => t.Status.Should().Be(TransactionStatus.Completed));
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce_WhenRequestIsValid()
    {
        // Arrange
        SetupValidSingleItemTransfer();
        var command = new CompleteWarehouseTransferCommand(_transferId, Guid.NewGuid());

        // Act
        await _handler.Handle(command, default);

        // Assert — single round-trip regardless of item count
        _stockTransactionRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    /// <summary>
    /// Configures a single valid item transfer and returns the entity references
    /// so tests can inspect mutations after the handler runs.
    /// </summary>
    private (WarehouseItemEntity fromItem, WarehouseItemEntity toItem, List<WarehouseTransferItemEntity> transferItems)
        SetupValidSingleItemTransfer(
            decimal fromQty = 20m,
            decimal toQty = 5m,
            decimal transferQty = 5m)
    {
        var ingredientId = Guid.NewGuid();
        var transferItemId = Guid.NewGuid();

        var transferItems = new List<WarehouseTransferItemEntity>
        {
            BuildTransferItem(transferItemId, ingredientId, quantity: transferQty)
        };

        SetupTransferItems(transferItems);

        var fromItem = BuildWarehouseItem(transferItemId, _fromWarehouseId, ingredientId, quantity: fromQty);
        var toItem = BuildWarehouseItem(Guid.NewGuid(), _toWarehouseId, ingredientId, quantity: toQty);

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(transferItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromItem);

        SetupWarehouseItemQueryable(new List<WarehouseItemEntity> { toItem });

        return (fromItem, toItem, transferItems);
    }

    private void SetupTransferItems(List<WarehouseTransferItemEntity> items)
    {
        var mock = items.BuildMock<WarehouseTransferItemEntity>();
        _transferItemRepositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(mock);
    }

    private void SetupWarehouseItemQueryable(List<WarehouseItemEntity> items)
    {
        var mock = items.BuildMock<WarehouseItemEntity>();
        _warehouseItemRepositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(mock);
    }

    private static WarehouseTransferItemEntity BuildTransferItem(
        Guid warehouseItemId,
        Guid ingredientId,
        decimal quantity = 5m,
        decimal unitPrice = 10m
    ) => new()
    {
        Id = Guid.NewGuid(),
        WarehouseItemId = warehouseItemId,
        IngredientId = ingredientId,
        Quantity = quantity,
        UnitPrice = unitPrice,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    private static WarehouseItemEntity BuildWarehouseItem(
        Guid id,
        Guid warehouseId,
        Guid ingredientId,
        decimal quantity = 20m,
        decimal basePrice = 10m,
        string unit = "kg"
    ) => new()
    {
        Id = id,
        WarehouseId = warehouseId,
        IngredientId = ingredientId,
        Unit = unit,
        Quantity = quantity,
        BasePrice = basePrice,
        IsDeleted = false,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
}