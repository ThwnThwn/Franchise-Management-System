using FluentAssertions;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.StockTransactions.Query.GetAll;
using WarehouseService.Application.Features.StockTransactions.Query.GetStockTransactions;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using Xunit;
using StockTransactionEntity = WarehouseService.Domain.Entities.StockTransaction;
using WarehouseItemEntity = WarehouseService.Domain.Entities.WarehouseItem;

namespace WarehouseService.Application.Tests.Features.StockTransactions;

public class GetStockTransactionsQueryHandlerTests
{
    private readonly Mock<IStockTransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _itemRepositoryMock;
    private readonly GetStockTransactionsQueryHandler _handler;

    public GetStockTransactionsQueryHandlerTests()
    {
        _transactionRepositoryMock = new Mock<IStockTransactionRepository>();
        _itemRepositoryMock = new Mock<IWarehouseItemRepository>();

        _handler = new GetStockTransactionsQueryHandler(
            _transactionRepositoryMock.Object,
            _itemRepositoryMock.Object
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — OPTIONAL FILTERS (each has HasValue=true and HasValue=false)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnAll_WhenNoFiltersAreProvided()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId),
            BuildTransaction(warehouseItemId: warehouseItemId),
            BuildTransaction(warehouseItemId: warehouseItemId)
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };

        SetupQueryables(transactions, items);

        var query = BuildQuery();

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldFilterByWarehouseId_WhenProvided()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseId: warehouseId,     warehouseItemId: warehouseItemId),
            BuildTransaction(warehouseId: Guid.NewGuid(),  warehouseItemId: warehouseItemId), // excluded
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(warehouseId: warehouseId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().AllSatisfy(t => t.WarehouseId.Should().Be(warehouseId));
    }

    [Fact]
    public async Task Handle_ShouldNotFilterByWarehouseId_WhenNull()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId),
            BuildTransaction(warehouseItemId: warehouseItemId)
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(warehouseId: null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByWarehouseItemId_WhenProvided()
    {
        // Arrange
        var targetItemId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: targetItemId),
            BuildTransaction(warehouseItemId: otherId)       // excluded
        };
        var items = new List<WarehouseItemEntity>
        {
            BuildWarehouseItem(targetItemId),
            BuildWarehouseItem(otherId)
        };
        SetupQueryables(transactions, items);

        var query = BuildQuery(warehouseItemId: targetItemId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].WarehouseItemId.Should().Be(targetItemId);
    }

    [Fact]
    public async Task Handle_ShouldNotFilterByWarehouseItemId_WhenNull()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: id1),
            BuildTransaction(warehouseItemId: id2)
        };
        var items = new List<WarehouseItemEntity>
        {
            BuildWarehouseItem(id1),
            BuildWarehouseItem(id2)
        };
        SetupQueryables(transactions, items);

        var query = BuildQuery(warehouseItemId: null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByType_WhenProvided()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, type: TransactionType.StockIn),
            BuildTransaction(warehouseItemId: warehouseItemId, type: TransactionType.StockOut)  // excluded
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(type: TransactionType.StockIn);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].Type.Should().Be(TransactionType.StockIn);
    }

    [Fact]
    public async Task Handle_ShouldNotFilterByType_WhenNull()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, type: TransactionType.StockIn),
            BuildTransaction(warehouseItemId: warehouseItemId, type: TransactionType.StockOut)
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(type: null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus_WhenProvided()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, status: TransactionStatus.Pending),
            BuildTransaction(warehouseItemId: warehouseItemId, status: TransactionStatus.Completed) // excluded
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(status: TransactionStatus.Pending);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].Status.Should().Be(TransactionStatus.Pending);
    }

    [Fact]
    public async Task Handle_ShouldNotFilterByStatus_WhenNull()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, status: TransactionStatus.Pending),
            BuildTransaction(warehouseItemId: warehouseItemId, status: TransactionStatus.Completed)
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(status: null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterBySupplierId_WhenProvided()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, supplierId: supplierId),
            BuildTransaction(warehouseItemId: warehouseItemId, supplierId: Guid.NewGuid()) // excluded
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(supplierId: supplierId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].SupplierId.Should().Be(supplierId);
    }

    [Fact]
    public async Task Handle_ShouldNotFilterBySupplierId_WhenNull()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, supplierId: Guid.NewGuid()),
            BuildTransaction(warehouseItemId: warehouseItemId, supplierId: Guid.NewGuid())
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(supplierId: null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByReferenceCode_WhenProvided()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, referenceCode: "PO-001"),
            BuildTransaction(warehouseItemId: warehouseItemId, referenceCode: "PO-999") // excluded
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(referenceCode: "PO-001");

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].ReferenceCode.Should().Be("PO-001");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldNotFilterByReferenceCode_WhenNullOrWhiteSpace(string? referenceCode)
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, referenceCode: "PO-001"),
            BuildTransaction(warehouseItemId: warehouseItemId, referenceCode: "PO-002")
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(referenceCode: referenceCode);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByFromDate_WhenProvided()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var fromDate = DateTimeOffset.UtcNow.AddDays(-3);
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: DateTimeOffset.UtcNow.AddDays(-1)),  // after fromDate — included
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: DateTimeOffset.UtcNow.AddDays(-5))   // before fromDate — excluded
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(fromDate: fromDate);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().AllSatisfy(t => t.CreatedAt.Should().BeOnOrAfter(fromDate));
    }

    [Fact]
    public async Task Handle_ShouldNotFilterByFromDate_WhenNull()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: DateTimeOffset.UtcNow.AddDays(-10)),
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: DateTimeOffset.UtcNow.AddDays(-1))
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(fromDate: null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByToDate_WhenProvided()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var toDate = DateTimeOffset.UtcNow.AddDays(-3);
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: DateTimeOffset.UtcNow.AddDays(-5)),  // before toDate — included
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: DateTimeOffset.UtcNow.AddDays(-1))   // after toDate — excluded
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(toDate: toDate);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().AllSatisfy(t => t.CreatedAt.Should().BeOnOrBefore(toDate));
    }

    [Fact]
    public async Task Handle_ShouldNotFilterByToDate_WhenNull()
    {
        // Arrange
        var warehouseItemId = Guid.NewGuid();
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: DateTimeOffset.UtcNow.AddDays(-10)),
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: DateTimeOffset.UtcNow.AddDays(-1))
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(toDate: null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — COMBINED FILTERS
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldApplyCombinedFilters_WhenMultipleFiltersProvided()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var warehouseItemId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var fromDate = DateTimeOffset.UtcNow.AddDays(-5);
        var toDate = DateTimeOffset.UtcNow;

        var matchingItem = BuildWarehouseItem(warehouseItemId);

        var transactions = new List<StockTransactionEntity>
        {
            // passes all filters
            BuildTransaction(warehouseId: warehouseId, warehouseItemId: warehouseItemId,
                type: TransactionType.StockIn, status: TransactionStatus.Pending,
                supplierId: supplierId, referenceCode: "PO-001",
                createdAt: DateTimeOffset.UtcNow.AddDays(-2)),
            // fails type filter
            BuildTransaction(warehouseId: warehouseId, warehouseItemId: warehouseItemId,
                type: TransactionType.StockOut, status: TransactionStatus.Pending,
                supplierId: supplierId, referenceCode: "PO-001",
                createdAt: DateTimeOffset.UtcNow.AddDays(-2)),
            // fails date filter
            BuildTransaction(warehouseId: warehouseId, warehouseItemId: warehouseItemId,
                type: TransactionType.StockIn, status: TransactionStatus.Pending,
                supplierId: supplierId, referenceCode: "PO-001",
                createdAt: DateTimeOffset.UtcNow.AddDays(-10))
        };
        var items = new List<WarehouseItemEntity> { matchingItem };
        SetupQueryables(transactions, items);

        var query = BuildQuery(
            warehouseId: warehouseId,
            type: TransactionType.StockIn,
            status: TransactionStatus.Pending,
            supplierId: supplierId,
            referenceCode: "PO-001",
            fromDate: fromDate,
            toDate: toDate);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert — only first transaction passes all filters
        result.TotalCount.Should().Be(1);
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — PAGINATION & METADATA
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnCorrectPage_WhenPaginationIsApplied()
    {
        // Arrange — 5 transactions, ordered desc by CreatedAt, page 2 of size 2
        var warehouseItemId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var transactions = Enumerable.Range(1, 5)
            .Select(i => BuildTransaction(
                warehouseItemId: warehouseItemId,
                createdAt: now.AddMinutes(-i)))   // t1=newest … t5=oldest
            .ToList();
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(page: 2, pageSize: 2);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert — page 2 of desc-ordered list = 3rd and 4th newest
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectTotalPages_WhenResultsDontDivideEvenly()
    {
        // Arrange — 7 items, PageSize 3 → ceil(7/3) = 3
        var warehouseItemId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 7)
            .Select(_ => BuildTransaction(warehouseItemId: warehouseItemId))
            .ToList();
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(page: 1, pageSize: 3);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectTotalPages_WhenResultsDivideEvenly()
    {
        // Arrange — 6 items, PageSize 3 → exactly 2 pages
        var warehouseItemId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 6)
            .Select(_ => BuildTransaction(warehouseItemId: warehouseItemId))
            .ToList();
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery(page: 1, pageSize: 3);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoTransactionsExist()
    {
        // Arrange
        SetupQueryables(new List<StockTransactionEntity>(), new List<WarehouseItemEntity>());

        var query = BuildQuery();

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — DTO FIELD MAPPING (includes Join to WarehouseItem for IngredientId)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldMapAllDtoFields_Correctly()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var warehouseItemId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);

        var transaction = new StockTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionCode = "SI-20260101-ABCDEF",
            WarehouseId = warehouseId,
            WarehouseItemId = warehouseItemId,
            Type = TransactionType.StockIn,
            Quantity = 10m,
            UnitPrice = 25m,
            TotalPrice = 250m,
            SupplierId = supplierId,
            ReferenceCode = "PO-MAP-001",
            WarehouseTransferId = null,
            Note = "test note",
            Status = TransactionStatus.Pending,
            CreatedBy = createdBy,
            IsDeleted = false,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
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
            UpdatedAt = createdAt
        };

        SetupQueryables(
            new List<StockTransactionEntity> { transaction },
            new List<WarehouseItemEntity> { warehouseItem });

        var query = BuildQuery();

        // Act
        var result = await _handler.Handle(query, default);

        // Assert — every field including IngredientId from the Join
        var dto = result.Items[0];
        dto.Id.Should().Be(transaction.Id);
        dto.TransactionCode.Should().Be("SI-20260101-ABCDEF");
        dto.WarehouseId.Should().Be(warehouseId);
        dto.WarehouseItemId.Should().Be(warehouseItemId);
        dto.IngredientId.Should().Be(ingredientId);       // comes from the Join
        dto.Type.Should().Be(TransactionType.StockIn);
        dto.Quantity.Should().Be(10m);
        dto.UnitPrice.Should().Be(25m);
        dto.TotalPrice.Should().Be(250m);
        dto.SupplierId.Should().Be(supplierId);
        dto.ReferenceCode.Should().Be("PO-MAP-001");
        dto.WarehouseTransferId.Should().BeNull();
        dto.Note.Should().Be("test note");
        dto.Status.Should().Be(TransactionStatus.Pending);
        dto.CreatedBy.Should().Be(createdBy);
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_ShouldReturnItemsOrderedByCreatedAt_Descending()
    {
        // Arrange — insert in ascending order; handler must reverse
        var warehouseItemId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var transactions = new List<StockTransactionEntity>
        {
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: now.AddDays(-3)),
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: now.AddDays(-1)),
            BuildTransaction(warehouseItemId: warehouseItemId, createdAt: now.AddDays(-2))
        };
        var items = new List<WarehouseItemEntity> { BuildWarehouseItem(warehouseItemId) };
        SetupQueryables(transactions, items);

        var query = BuildQuery();

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Items.Select(t => t.CreatedAt)
            .Should().BeInDescendingOrder();
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
        Guid? warehouseId = null,
        Guid? warehouseItemId = null,
        TransactionType type = TransactionType.StockIn,
        TransactionStatus status = TransactionStatus.Pending,
        Guid? supplierId = null,
        string? referenceCode = null,
        DateTimeOffset? createdAt = null
    ) => new()
    {
        Id = Guid.NewGuid(),
        TransactionCode = $"SI-TEST-{Guid.NewGuid().ToString()[..6].ToUpper()}",
        WarehouseId = warehouseId ?? Guid.NewGuid(),
        WarehouseItemId = warehouseItemId ?? Guid.NewGuid(),
        Type = type,
        Quantity = 5m,
        UnitPrice = 10m,
        TotalPrice = 50m,
        SupplierId = supplierId,
        ReferenceCode = referenceCode,
        WarehouseTransferId = null,
        Note = null,
        Status = status,
        CreatedBy = Guid.NewGuid(),
        IsDeleted = false,
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
        UpdatedAt = createdAt ?? DateTimeOffset.UtcNow
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

    private static GetStockTransactionsQuery BuildQuery(
        Guid? warehouseId = null,
        Guid? warehouseItemId = null,
        TransactionType? type = null,
        TransactionStatus? status = null,
        Guid? supplierId = null,
        string? referenceCode = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int page = 1,
        int pageSize = 10
    ) => new GetStockTransactionsQuery(
        WarehouseId: warehouseId,
        WarehouseItemId: warehouseItemId,
        Type: type,
        Status: status,
        SupplierId: supplierId,
        ReferenceCode: referenceCode,
        FromDate: fromDate,
        ToDate: toDate,
        Page: page,
        PageSize: pageSize
    );
}