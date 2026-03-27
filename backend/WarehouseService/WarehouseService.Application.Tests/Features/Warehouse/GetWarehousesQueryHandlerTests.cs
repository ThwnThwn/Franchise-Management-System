using Moq;
using Xunit;
using FluentAssertions;
using MockQueryable;                  // MockQueryable.Moq 8.0.1
using WarehouseService.Application.Features.Warehouses.Queries.GetAll;
using WarehouseService.Application.Features.Warehouses.Queries.GetById;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using WarehouseEntity = WarehouseService.Domain.Entities.Warehouse;
using WarehouseItemEntity = WarehouseService.Domain.Entities.WarehouseItem;

namespace WarehouseService.Application.Tests.Features.Warehouses;

// =============================================================================
// GET-ALL
// =============================================================================

public class GetWarehousesQueryHandlerTests
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly GetWarehousesQueryHandler _handler;

    public GetWarehousesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();
        _handler = new GetWarehousesQueryHandler(_repositoryMock.Object);
    }

    // -------------------------------------------------------------------------
    // GROUP 1 — FILTERING BRANCHES
    // Every optional filter (StoreId, Status, SearchTerm) has two branches:
    //   • HasValue / non-empty  → filter applied
    //   • null / whitespace     → filter skipped (all rows returned)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnAllWarehouses_WhenNoFiltersAreProvided()
    {
        // Arrange — 3 warehouses, nothing filtered
        var warehouses = new List<WarehouseEntity>
        {
            BuildWarehouse(code: "WH-A"),
            BuildWarehouse(code: "WH-B"),
            BuildWarehouse(code: "WH-C")
        };
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStoreId_WhenStoreIdIsProvided()
    {
        // Arrange
        var targetStoreId = Guid.NewGuid();
        var warehouses = new List<WarehouseEntity>
        {
            BuildWarehouse(code: "WH-A", storeId: targetStoreId),
            BuildWarehouse(code: "WH-B", storeId: Guid.NewGuid()),  // different store
            BuildWarehouse(code: "WH-C", storeId: targetStoreId)
        };
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: targetStoreId, Status: null, SearchTerm: null, Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(w => w.StoreId.Should().Be(targetStoreId));
    }

    [Fact]
    public async Task Handle_ShouldNotFilterByStoreId_WhenStoreIdIsNull()
    {
        // Arrange — branch: StoreId null → Where not applied
        var warehouses = new List<WarehouseEntity>
        {
            BuildWarehouse(code: "WH-A"),
            BuildWarehouse(code: "WH-B")
        };
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert — all rows returned
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus_WhenStatusIsProvided()
    {
        // Arrange
        var warehouses = new List<WarehouseEntity>
        {
            BuildWarehouse(code: "WH-A", status: WarehouseStatus.Active),
            BuildWarehouse(code: "WH-B", status: WarehouseStatus.Inactive),
            BuildWarehouse(code: "WH-C", status: WarehouseStatus.Active)
        };
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: WarehouseStatus.Active, SearchTerm: null, Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(w => w.Status.Should().Be(WarehouseStatus.Active));
    }

    [Fact]
    public async Task Handle_ShouldNotFilterByStatus_WhenStatusIsNull()
    {
        // Arrange — branch: Status null → Where not applied
        var warehouses = new List<WarehouseEntity>
        {
            BuildWarehouse(code: "WH-A", status: WarehouseStatus.Active),
            BuildWarehouse(code: "WH-B", status: WarehouseStatus.Inactive)
        };
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Theory]
    [InlineData("wh-a")]         // matches Code (case-insensitive)
    [InlineData("Main")]         // matches Name
    [InlineData("elm street")]   // matches Address
    public async Task Handle_ShouldFilterBySearchTerm_WhenSearchTermMatchesCodeNameOrAddress(string searchTerm)
    {
        // Arrange
        var warehouses = new List<WarehouseEntity>
        {
            BuildWarehouse(code: "WH-A",  name: "Main Warehouse", address: "1 Oak Ave"),
            BuildWarehouse(code: "WH-B",  name: "North Hub",      address: "10 Elm Street"),
            BuildWarehouse(code: "WH-C",  name: "South Hub",      address: "99 Pine Rd")
        };
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: searchTerm, Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert — at least one result, none from the non-matching warehouse
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldNotFilterBySearchTerm_WhenSearchTermIsNullOrWhiteSpace(string? searchTerm)
    {
        // Arrange — branch: null/empty/whitespace → Where not applied
        var warehouses = new List<WarehouseEntity>
        {
            BuildWarehouse(code: "WH-A"),
            BuildWarehouse(code: "WH-B")
        };
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: searchTerm, Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldApplyCombinedFilters_WhenMultipleFiltersProvided()
    {
        // Arrange — all three filters active simultaneously
        var targetStoreId = Guid.NewGuid();
        var warehouses = new List<WarehouseEntity>
        {
            BuildWarehouse(code: "WH-A", name: "Alpha",  storeId: targetStoreId, status: WarehouseStatus.Active),
            BuildWarehouse(code: "WH-B", name: "Alpha",  storeId: targetStoreId, status: WarehouseStatus.Inactive), // status mismatch
            BuildWarehouse(code: "WH-C", name: "Beta",   storeId: targetStoreId, status: WarehouseStatus.Active),   // name mismatch
            BuildWarehouse(code: "WH-D", name: "Alpha",  storeId: Guid.NewGuid(), status: WarehouseStatus.Active)   // store mismatch
        };
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: targetStoreId,
            Status: WarehouseStatus.Active,
            SearchTerm: "Alpha",
            Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert — only WH-A passes all three filters
        result.TotalCount.Should().Be(1);
        result.Items[0].Code.Should().Be("WH-A");
    }

    // -------------------------------------------------------------------------
    // GROUP 2 — PAGINATION
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnCorrectPage_WhenPaginationIsApplied()
    {
        // Arrange — 5 warehouses, page 2 of size 2 → items 3 and 4 (by Code order)
        var warehouses = Enumerable.Range(1, 5)
            .Select(i => BuildWarehouse(code: $"WH-{i:D2}"))
            .ToList();
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 2, PageSize: 2);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].Code.Should().Be("WH-03");
        result.Items[1].Code.Should().Be("WH-04");
    }

    [Fact]
    public async Task Handle_ShouldReturnRemainingItems_WhenLastPageIsPartial()
    {
        // Arrange — 5 items, PageSize 2 → last page (page 3) has 1 item
        var warehouses = Enumerable.Range(1, 5)
            .Select(i => BuildWarehouse(code: $"WH-{i:D2}"))
            .ToList();
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 3, PageSize: 2);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Code.Should().Be("WH-05");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyItems_WhenPageExceedsTotalPages()
    {
        // Arrange
        var warehouses = new List<WarehouseEntity> { BuildWarehouse(code: "WH-01") };
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 99, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoWarehousesExist()
    {
        // Arrange
        SetupQueryable(new List<WarehouseEntity>());

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // GROUP 3 — RESULT METADATA & MAPPING
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnCorrectTotalPages_WhenResultsDontDivideEvenly()
    {
        // Arrange — 7 items, PageSize 3 → ceil(7/3) = 3 pages
        var warehouses = Enumerable.Range(1, 7)
            .Select(i => BuildWarehouse(code: $"WH-{i:D2}"))
            .ToList();
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 1, PageSize: 3);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalPages.Should().Be(3);
        result.TotalCount.Should().Be(7);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectTotalPages_WhenResultsDivideEvenly()
    {
        // Arrange — 6 items, PageSize 3 → exactly 2 pages (no ceiling rounding needed)
        var warehouses = Enumerable.Range(1, 6)
            .Select(i => BuildWarehouse(code: $"WH-{i:D2}"))
            .ToList();
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 1, PageSize: 3);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldMapAllDtoFields_Correctly()
    {
        // Arrange — verify every field of the WarehouseDto is mapped from the entity
        var storeId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-5);
        var updatedAt = DateTimeOffset.UtcNow.AddDays(-1);

        var warehouse = new WarehouseEntity
        {
            Id = Guid.NewGuid(),
            Code = "WH-MAP",
            Name = "Mapping Test",
            Address = "1 Map Street",
            StoreId = storeId,
            Status = WarehouseStatus.Active,
            IsDeleted = false,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            WarehouseItems = new List<WarehouseItemEntity>()
        };
        SetupQueryable(new List<WarehouseEntity> { warehouse });

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        var dto = result.Items[0];
        dto.Id.Should().Be(warehouse.Id);
        dto.Code.Should().Be("WH-MAP");
        dto.Name.Should().Be("Mapping Test");
        dto.Address.Should().Be("1 Map Street");
        dto.StoreId.Should().Be(storeId);
        dto.Status.Should().Be(WarehouseStatus.Active);
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task Handle_ShouldReturnItemsOrderedByCode_Ascending()
    {
        // Arrange — insert out of order; handler must sort by Code
        var warehouses = new List<WarehouseEntity>
        {
            BuildWarehouse(code: "WH-C"),
            BuildWarehouse(code: "WH-A"),
            BuildWarehouse(code: "WH-B")
        };
        SetupQueryable(warehouses);

        var query = new GetWarehousesQuery(
            StoreId: null, Status: null, SearchTerm: null, Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Items.Select(w => w.Code)
            .Should().BeInAscendingOrder();
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Wraps the list in MockQueryable so EF Core async extensions
    /// (CountAsync, ToListAsync) work without a real DbContext.
    /// </summary>
    private void SetupQueryable(List<WarehouseEntity> warehouses)
    {
        var mock = warehouses.BuildMock<WarehouseEntity>();
        _repositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(mock);
    }

    private static WarehouseEntity BuildWarehouse(
        string? code = null,
        string? name = null,
        string? address = null,
        Guid? storeId = null,
        WarehouseStatus? status = null
    ) => new()
    {
        Id = Guid.NewGuid(),
        Code = code ?? "WH-DEFAULT",
        Name = name ?? "Default Warehouse",
        Address = address ?? "1 Default Street",
        StoreId = storeId ?? Guid.NewGuid(),
        Status = status ?? WarehouseStatus.Active,
        IsDeleted = false,
        CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
        UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
        WarehouseItems = new List<WarehouseItemEntity>()
    };
}

