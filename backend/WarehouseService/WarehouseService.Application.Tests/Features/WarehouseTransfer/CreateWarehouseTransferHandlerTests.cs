using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CreateWarehouseTransfer;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using WarehouseEntity = WarehouseService.Domain.Entities.Warehouse;
using WarehouseItemEntity = WarehouseService.Domain.Entities.WarehouseItem;
using WarehouseTransferEntity = WarehouseService.Domain.Entities.WarehouseTransfer;
using WarehouseTransferItemEntity = WarehouseService.Domain.Entities.WarehouseTransferItem;

namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

public class CreateWarehouseTransferHandlerTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _itemRepositoryMock;
    private readonly Mock<IWarehouseTransferRepository> _transferRepositoryMock;
    private readonly Mock<IWarehouseTransferItemRepository> _transferItemRepositoryMock;
    private readonly CreateWarehouseTransferCommandHandler _handler;

    // Fixed warehouse IDs reused across tests
    private readonly Guid _fromWarehouseId = Guid.NewGuid();
    private readonly Guid _toWarehouseId = Guid.NewGuid();

    public CreateWarehouseTransferHandlerTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _itemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _transferRepositoryMock = new Mock<IWarehouseTransferRepository>();
        _transferItemRepositoryMock = new Mock<IWarehouseTransferItemRepository>();

        _handler = new CreateWarehouseTransferCommandHandler(
            _warehouseRepositoryMock.Object,
            _itemRepositoryMock.Object,
            _transferRepositoryMock.Object,
            _transferItemRepositoryMock.Object
        );

        // Default: both warehouses exist
        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(_fromWarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WarehouseEntity { Id = _fromWarehouseId });

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(_toWarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WarehouseEntity { Id = _toWarehouseId });

        // Default: no-op persistence
        _transferRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<WarehouseTransferEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transferItemRepositoryMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<WarehouseTransferItemEntity>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transferItemRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — WAREHOUSE VALIDATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenFromWarehouseDoesNotExist()
    {
        // Arrange
        var fromId = Guid.NewGuid();

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseEntity?)null);

        var command = BuildCommand(fromWarehouseId: fromId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{fromId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenToWarehouseDoesNotExist()
    {
        // Arrange
        var toId = Guid.NewGuid();

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(toId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseEntity?)null);

        var command = BuildCommand(toWarehouseId: toId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{toId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenFromAndToWarehouseAreSame()
    {
        // Arrange — same ID for both
        var warehouseId = Guid.NewGuid();

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WarehouseEntity { Id = warehouseId });

        var command = BuildCommand(fromWarehouseId: warehouseId, toWarehouseId: warehouseId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*different*");
    }

    [Fact]
    public async Task Handle_ShouldCheckFromWarehouse_BeforeToWarehouse()
    {
        // Arrange — from missing, to also missing; only from-not-found error fires
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseEntity?)null);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(toId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseEntity?)null);

        var command = BuildCommand(fromWarehouseId: fromId, toWarehouseId: toId);

        // Act & Assert — message contains fromId, not toId
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{fromId}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — ITEM VALIDATION (per-item guards inside foreach)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenWarehouseItemDoesNotExist()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseItemEntity?)null);

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, 5m) });

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{itemId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenItemDoesNotBelongToFromWarehouse()
    {
        // Arrange — item belongs to a different warehouse, not fromWarehouseId
        var itemId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WarehouseItemEntity
            {
                Id = itemId,
                WarehouseId = Guid.NewGuid(),   // wrong warehouse
                Quantity = 20m,
                BasePrice = 5m,
                Unit = "kg",
                IngredientId = Guid.NewGuid()
            });

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, 5m) });

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{itemId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenRequestedQuantityExceedsStock()
    {
        // Arrange — item has 5 in stock, request wants 10
        var itemId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId, _fromWarehouseId, quantity: 5m));

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, Quantity: 10m) });

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{itemId}*");
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenRequestedQuantityEqualsStock()
    {
        // Arrange — exact boundary: requested == available
        var itemId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId, _fromWarehouseId, quantity: 10m));

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, Quantity: 10m) });

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenRequestedQuantityIsLessThanStock()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId, _fromWarehouseId, quantity: 50m));

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, Quantity: 10m) });

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().NotThrowAsync();
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — SUCCESSFUL CREATION & FIELD MAPPING
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithCorrectHeaderFields_WhenRequestIsValid()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId, _fromWarehouseId, quantity: 20m));

        var command = BuildCommand(
            items: new[] { new TransferItemDto(itemId, 5m) },
            reason: "Monthly rebalance",
            note: "Urgent",
            createdBy: createdBy);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.TransferCode.Should().StartWith("TRF-");
        result.FromWarehouseId.Should().Be(_fromWarehouseId);
        result.ToWarehouseId.Should().Be(_toWarehouseId);
        result.Status.Should().Be(TransferStatus.Pending);
        result.Reason.Should().Be("Monthly rebalance");
        result.Note.Should().Be("Urgent");
        result.CreatedBy.Should().Be(createdBy);
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldReturnResult_WithNullReasonAndNote_WhenNotProvided()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId, _fromWarehouseId));

        var command = BuildCommand(
            items: new[] { new TransferItemDto(itemId, 1m) },
            reason: null,
            note: null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Reason.Should().BeNull();
        result.Note.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectItemInfos_MappedFromWarehouseItems()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var warehouseItem = new WarehouseItemEntity
        {
            Id = itemId,
            WarehouseId = _fromWarehouseId,
            IngredientId = ingredientId,
            Unit = "litre",
            Quantity = 30m,
            BasePrice = 12.50m,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouseItem);

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, Quantity: 10m) });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert — TransferItemInfoDto fields come from WarehouseItem, not the request
        result.Items.Should().HaveCount(1);
        var itemInfo = result.Items[0];
        itemInfo.WarehouseItemId.Should().Be(itemId);
        itemInfo.IngredientId.Should().Be(ingredientId);
        itemInfo.Unit.Should().Be("litre");
        itemInfo.Quantity.Should().Be(10m);          // from request
        itemInfo.UnitPrice.Should().Be(12.50m);      // BasePrice from entity
    }

    [Fact]
    public async Task Handle_ShouldReturnMultipleItemInfos_WhenMultipleItemsRequested()
    {
        // Arrange
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId1, _fromWarehouseId, quantity: 20m));

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId2, _fromWarehouseId, quantity: 15m));

        var command = BuildCommand(items: new[]
        {
            new TransferItemDto(itemId1, 5m),
            new TransferItemDto(itemId2, 3m)
        });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Select(i => i.WarehouseItemId)
            .Should().BeEquivalentTo(new[] { itemId1, itemId2 });
    }

    [Fact]
    public async Task Handle_ShouldLinkTransferItemsToTransferId()
    {
        // Arrange — capture the entities passed to AddRangeAsync and verify the link
        var itemId = Guid.NewGuid();
        IEnumerable<WarehouseTransferItemEntity>? capturedItems = null;

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId, _fromWarehouseId));

        _transferItemRepositoryMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<WarehouseTransferItemEntity>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<WarehouseTransferItemEntity>, CancellationToken>((items, _) => capturedItems = items)
            .Returns(Task.CompletedTask);

        WarehouseTransferEntity? capturedTransfer = null;
        _transferRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<WarehouseTransferEntity>(), It.IsAny<CancellationToken>()))
            .Callback<WarehouseTransferEntity, CancellationToken>((t, _) => capturedTransfer = t)
            .Returns(Task.CompletedTask);

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, 5m) });

        // Act
        await _handler.Handle(command, default);

        // Assert — every transfer item points to the transfer header
        capturedTransfer.Should().NotBeNull();
        capturedItems.Should().NotBeNull();
        capturedItems!.Should().AllSatisfy(ti =>
            ti.WarehouseTransferId.Should().Be(capturedTransfer!.Id));
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — EMPTY ITEMS LIST (foreach body never executes)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnEmptyItemInfos_WhenItemsListIsEmpty()
    {
        // Arrange
        var command = BuildCommand(items: Array.Empty<TransferItemDto>());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldNotCallItemGetByIdAsync_WhenItemsListIsEmpty()
    {
        // Arrange
        var command = BuildCommand(items: Array.Empty<TransferItemDto>());

        // Act
        await _handler.Handle(command, default);

        // Assert
        _itemRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 5 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallAddAsync_OnTransferRepository_ExactlyOnce()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId, _fromWarehouseId));

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, 1m) });

        // Act
        await _handler.Handle(command, default);

        // Assert
        _transferRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<WarehouseTransferEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCallAddRangeAsync_OnTransferItemRepository_ExactlyOnce()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId, _fromWarehouseId));

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, 1m) });

        // Act
        await _handler.Handle(command, default);

        // Assert
        _transferItemRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IEnumerable<WarehouseTransferItemEntity>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId, _fromWarehouseId));

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, 1m) });

        // Act
        await _handler.Handle(command, default);

        // Assert — single round-trip for header + all items
        _transferItemRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenFromWarehouseDoesNotExist()
    {
        // Arrange
        var fromId = Guid.NewGuid();

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseEntity?)null);

        var command = BuildCommand(fromWarehouseId: fromId);

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _transferItemRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenItemStockIsInsufficient()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildWarehouseItem(itemId, _fromWarehouseId, quantity: 2m));

        var command = BuildCommand(items: new[] { new TransferItemDto(itemId, Quantity: 10m) });

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _transferItemRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    private CreateWarehouseTransferCommand BuildCommand(
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        IEnumerable<TransferItemDto>? items = null,
        string? reason = null,
        string? note = null,
        Guid? createdBy = null
    ) => new CreateWarehouseTransferCommand(
        FromWarehouseId: fromWarehouseId ?? _fromWarehouseId,
        ToWarehouseId: toWarehouseId ?? _toWarehouseId,
        Items: (items ?? Array.Empty<TransferItemDto>()).ToList(),
        Reason: reason,
        Note: note,
        CreatedBy: createdBy ?? Guid.NewGuid()
    );

    private static WarehouseItemEntity BuildWarehouseItem(
        Guid id,
        Guid warehouseId,
        decimal quantity = 20m,
        decimal basePrice = 10m
    ) => new()
    {
        Id = id,
        WarehouseId = warehouseId,
        IngredientId = Guid.NewGuid(),
        Unit = "kg",
        Quantity = quantity,
        BasePrice = basePrice,
        IsDeleted = false,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
}