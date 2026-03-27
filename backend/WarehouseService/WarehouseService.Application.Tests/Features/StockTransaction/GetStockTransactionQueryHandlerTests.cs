using FluentAssertions;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.StockTransactions.Query.GetById;
using WarehouseService.Application.Features.StockTransactions.Query.GetStockTransaction;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using Xunit;
using StockTransactionEntity = WarehouseService.Domain.Entities.StockTransaction;
using WarehouseItemEntity = WarehouseService.Domain.Entities.WarehouseItem;

namespace WarehouseService.Application.Tests.Features.StockTransactions;

public class GetStockTransactionQueryHandlerTests
{
    private readonly Mock<IStockTransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _itemRepositoryMock;
    private readonly GetStockTransactionQueryHandler _handler;

    public GetStockTransactionQueryHandlerTests()
    {
        _transactionRepositoryMock = new Mock<IStockTransactionRepository>();
        _itemRepositoryMock = new Mock<IWarehouseItemRepository>();

        _handler = new GetStockTransactionQueryHandler(
            _transactionRepositoryMock.Object,
            _itemRepositoryMock.Object
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — NOT FOUND BRANCH
    // The Join returns nothing when either the transaction ID doesn't exist
    // or there is no matching WarehouseItem — FirstOrDefaultAsync returns null.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransactionDoesNotExist()
    {
        // Arrange — empty transaction queryable, so Join yields nothing
        var id = Guid.NewGuid();

        SetupQueryables(
            transactions: new List<StockTransactionEntity>(),
            items: new List<WarehouseItemEntity>());

        var query = new GetStockTransactionQuery(id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(query, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransactionExistsButWarehouseItemIsMissing()
    {
        // Arrange — transaction exists but its WarehouseItemId has no matching item,
        //           so the Join yields zero rows → FirstOrDefaultAsync returns null
        var id = Guid.NewGuid();
        var warehouseItemId = Guid.NewGuid();

        var transaction = BuildTransaction(id: id, warehouseItemId: warehouseItemId);

        SetupQueryables(
            transactions: new List<StockTransactionEntity> { transaction },
            items: new List<WarehouseItemEntity>());   // no matching item

        var query = new GetStockTransactionQuery(id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(query, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — FOUND BRANCH: MAPPING
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithAllFieldsMapped_WhenTransactionExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var warehouseItemId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var warehouseTransferId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-3);
        var updatedAt = DateTimeOffset.UtcNow.AddDays(-1);

        var transaction = new StockTransactionEntity
        {
            Id = id,
            TransactionCode = "SI-20260101-ABCDEF",
            WarehouseId = warehouseId,
            WarehouseItemId = warehouseItemId,
            Type = TransactionType.StockIn,
            Quantity = 10m,
            UnitPrice = 25m,
            TotalPrice = 250m,
            SupplierId = supplierId,
            ReferenceCode = "PO-001",
            WarehouseTransferId = warehouseTransferId,
            Note = "test note",
            Status = TransactionStatus.Pending,
            CreatedBy = createdBy,
            IsDeleted = false,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        var warehouseItem = new WarehouseItemEntity
        {
            Id = warehouseItemId,
            WarehouseId = warehouseId,
            IngredientId = ingredientId,
            Unit = "kg",
            Quantity = 50m,
            BasePrice = 20m,
            IsDeleted = false,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        SetupQueryables(
            new List<StockTransactionEntity> { transaction },
            new List<WarehouseItemEntity> { warehouseItem });

        var query = new GetStockTransactionQuery(id);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert — every field including IngredientId from the Join
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.TransactionCode.Should().Be("SI-20260101-ABCDEF");
        result.WarehouseId.Should().Be(warehouseId);
        result.WarehouseItemId.Should().Be(warehouseItemId);
        result.IngredientId.Should().Be(ingredientId);          // comes from Join
        result.Type.Should().Be(TransactionType.StockIn);
        result.Quantity.Should().Be(10m);
        result.UnitPrice.Should().Be(25m);
        result.TotalPrice.Should().Be(250m);
        result.SupplierId.Should().Be(supplierId);
        result.ReferenceCode.Should().Be("PO-001");
        result.WarehouseTransferId.Should().Be(warehouseTransferId);
        result.Note.Should().Be("test note");
        result.Status.Should().Be(TransactionStatus.Pending);
        result.CreatedBy.Should().Be(createdBy);
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task Handle_ShouldReturnNullOptionalFields_WhenTransactionHasNoSupplierOrReference()
    {
        // Arrange — nullable fields left null
        var id = Guid.NewGuid();
        var warehouseItemId = Guid.NewGuid();

        var transaction = BuildTransaction(
            id: id,
            warehouseItemId: warehouseItemId,
            supplierId: null,
            referenceCode: null,
            transferId: null,
            note: null);

        var warehouseItem = BuildWarehouseItem(warehouseItemId);

        SetupQueryables(
            new List<StockTransactionEntity> { transaction },
            new List<WarehouseItemEntity> { warehouseItem });

        var query = new GetStockTransactionQuery(id);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert — nullable fields are null
        result.SupplierId.Should().BeNull();
        result.ReferenceCode.Should().BeNull();
        result.WarehouseTransferId.Should().BeNull();
        result.Note.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnTransactionMatchingRequestId()
    {
        // Arrange — two transactions in the queryable; only the requested one should be returned
        var targetId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var warehouseItemId = Guid.NewGuid();

        var target = BuildTransaction(id: targetId, warehouseItemId: warehouseItemId);
        var other = BuildTransaction(id: otherId, warehouseItemId: warehouseItemId);

        var warehouseItem = BuildWarehouseItem(warehouseItemId);

        SetupQueryables(
            new List<StockTransactionEntity> { target, other },
            new List<WarehouseItemEntity> { warehouseItem });

        var query = new GetStockTransactionQuery(targetId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Id.Should().Be(targetId);
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    private void SetupQueryables(
        List<StockTransactionEntity> transactions,
        List<WarehouseItemEntity> items)
    {
        _transactionRepositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(transactions.BuildMock<StockTransactionEntity>());

        _itemRepositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(items.BuildMock<WarehouseItemEntity>());
    }

    private static StockTransactionEntity BuildTransaction(
        Guid? id = null,
        Guid? warehouseItemId = null,
        TransactionType type = TransactionType.StockIn,
        TransactionStatus status = TransactionStatus.Pending,
        Guid? supplierId = null,
        string? referenceCode = null,
        Guid? transferId = null,
        string? note = null
    ) => new()
    {
        Id = id ?? Guid.NewGuid(),
        TransactionCode = "SI-TEST-000000",
        WarehouseId = Guid.NewGuid(),
        WarehouseItemId = warehouseItemId ?? Guid.NewGuid(),
        Type = type,
        Quantity = 5m,
        UnitPrice = 10m,
        TotalPrice = 50m,
        SupplierId = supplierId,
        ReferenceCode = referenceCode,
        WarehouseTransferId = transferId,
        Note = note,
        Status = status,
        CreatedBy = Guid.NewGuid(),
        IsDeleted = false,
        CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
        UpdatedAt = DateTimeOffset.UtcNow
    };

    private static WarehouseItemEntity BuildWarehouseItem(Guid id) => new()
    {
        Id = id,
        WarehouseId = Guid.NewGuid(),
        IngredientId = Guid.NewGuid(),
        Unit = "kg",
        Quantity = 50m,
        BasePrice = 10m,
        IsDeleted = false,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
}