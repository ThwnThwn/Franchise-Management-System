using Moq;
using Xunit;
using FluentAssertions;
using MockQueryable;
using WarehouseService.Application.Features.WarehouseItems.Query.GetWarehouseItems;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using WarehouseItemEntity = WarehouseService.Domain.Entities.WarehouseItem;

namespace WarehouseService.Application.Tests.Features.WarehouseItems;

public class GetWarehouseItemsQueryHandlerTests
{
    private readonly Mock<IWarehouseItemRepository> _itemRepositoryMock;
    private readonly GetWarehouseItemsQueryHandler _handler;

    public GetWarehouseItemsQueryHandlerTests()
    {
        _itemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new GetWarehouseItemsQueryHandler(_itemRepositoryMock.Object);
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — BASE FILTER (WarehouseId is always applied)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnOnlyItemsForGivenWarehouse()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var items = new List<WarehouseItemEntity>
        {
            BuildItem(warehouseId: warehouseId),
            BuildItem(warehouseId: warehouseId),
            BuildItem(warehouseId: Guid.NewGuid())  // different warehouse — excluded
        };
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(i => i.WarehouseId.Should().Be(warehouseId));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoItemsBelongToWarehouse()
    {
        // Arrange
        var items = new List<WarehouseItemEntity>
        {
            BuildItem(warehouseId: Guid.NewGuid()),
            BuildItem(warehouseId: Guid.NewGuid())
        };
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — OPTIONAL FILTERS
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldFilterByIngredientId_WhenIngredientIdIsProvided()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var items = new List<WarehouseItemEntity>
        {
            BuildItem(warehouseId: warehouseId, ingredientId: ingredientId),
            BuildItem(warehouseId: warehouseId, ingredientId: Guid.NewGuid())  // different ingredient
        };
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId, ingredientId: ingredientId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].IngredientId.Should().Be(ingredientId);
    }

    [Fact]
    public async Task Handle_ShouldNotFilterByIngredientId_WhenIngredientIdIsNull()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var items = new List<WarehouseItemEntity>
        {
            BuildItem(warehouseId: warehouseId),
            BuildItem(warehouseId: warehouseId)
        };
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId, ingredientId: null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterLowStock_WhenLowStockIsTrue()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var items = new List<WarehouseItemEntity>
        {
            BuildItem(warehouseId: warehouseId, quantity: 5),    // low stock  ≤ 10
            BuildItem(warehouseId: warehouseId, quantity: 10),   // boundary   = 10
            BuildItem(warehouseId: warehouseId, quantity: 11)    // not low stock
        };
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId, lowStock: true);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(i => i.Quantity.Should().BeLessThanOrEqualTo(10));
    }

    [Fact]
    public async Task Handle_ShouldNotFilterLowStock_WhenLowStockIsFalse()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var items = new List<WarehouseItemEntity>
        {
            BuildItem(warehouseId: warehouseId, quantity: 5),
            BuildItem(warehouseId: warehouseId, quantity: 50)
        };
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId, lowStock: false);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert — both items returned
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldNotFilterLowStock_WhenLowStockIsNull()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var items = new List<WarehouseItemEntity>
        {
            BuildItem(warehouseId: warehouseId, quantity: 5),
            BuildItem(warehouseId: warehouseId, quantity: 50)
        };
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId, lowStock: null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterExpiringSoon_WhenExpiringSoonIsTrue()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var items = new List<WarehouseItemEntity>
        {
            BuildItem(warehouseId: warehouseId, expiryDate: DateTime.UtcNow.AddDays(10)),  // expires soon
            BuildItem(warehouseId: warehouseId, expiryDate: DateTime.UtcNow.AddDays(29)),  // expires soon (< 30)
            BuildItem(warehouseId: warehouseId, expiryDate: DateTime.UtcNow.AddDays(60)),  // not soon
            BuildItem(warehouseId: warehouseId, expiryDate: null)                          // no expiry — excluded
        };
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId, expiringSoon: true);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(i =>
            i.ExpiryDate.Should().NotBeNull().And
             .BeOnOrBefore(DateTime.UtcNow.AddDays(31))); // small tolerance for test timing
    }

    [Fact]
    public async Task Handle_ShouldNotFilterExpiringSoon_WhenExpiringSoonIsFalse()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var items = new List<WarehouseItemEntity>
        {
            BuildItem(warehouseId: warehouseId, expiryDate: DateTime.UtcNow.AddDays(5)),
            BuildItem(warehouseId: warehouseId, expiryDate: DateTime.UtcNow.AddDays(90))
        };
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId, expiringSoon: false);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldNotFilterExpiringSoon_WhenExpiringSoonIsNull()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var items = new List<WarehouseItemEntity>
        {
            BuildItem(warehouseId: warehouseId, expiryDate: DateTime.UtcNow.AddDays(5)),
            BuildItem(warehouseId: warehouseId, expiryDate: DateTime.UtcNow.AddDays(90))
        };
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId, expiringSoon: null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldApplyCombinedFilters_WhenMultipleFiltersProvided()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var items = new List<WarehouseItemEntity>
        {
            // passes all: correct ingredient + low stock + expiring soon
            BuildItem(warehouseId: warehouseId, ingredientId: ingredientId, quantity: 5,  expiryDate: DateTime.UtcNow.AddDays(10)),
            // fails ingredient filter
            BuildItem(warehouseId: warehouseId, ingredientId: Guid.NewGuid(),             quantity: 3,  expiryDate: DateTime.UtcNow.AddDays(5)),
            // fails low stock filter
            BuildItem(warehouseId: warehouseId, ingredientId: ingredientId, quantity: 50, expiryDate: DateTime.UtcNow.AddDays(10)),
            // fails expiring soon filter
            BuildItem(warehouseId: warehouseId, ingredientId: ingredientId, quantity: 5,  expiryDate: DateTime.UtcNow.AddDays(60))
        };
        SetupQueryable(items);

        var query = BuildQuery(
            warehouseId: warehouseId,
            ingredientId: ingredientId,
            lowStock: true,
            expiringSoon: true);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert — only the first item passes all three optional filters
        result.TotalCount.Should().Be(1);
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — PAGINATION & METADATA
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnCorrectPage_WhenPaginationIsApplied()
    {
        // Arrange — 5 items, page 2 of size 2
        var warehouseId = Guid.NewGuid();
        var items = Enumerable.Range(1, 5)
            .Select(i => BuildItem(warehouseId: warehouseId, quantity: i))
            .ToList();
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId, page: 2, pageSize: 2);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectTotalPages_WhenResultsDontDivideEvenly()
    {
        // Arrange — 7 items, PageSize 3 → ceil(7/3) = 3
        var warehouseId = Guid.NewGuid();
        var items = Enumerable.Range(1, 7)
            .Select(i => BuildItem(warehouseId: warehouseId))
            .ToList();
        SetupQueryable(items);

        var query = BuildQuery(warehouseId: warehouseId, page: 1, pageSize: 3);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoItemsExistForWarehouse()
    {
        // Arrange
        SetupQueryable(new List<WarehouseItemEntity>());

        var query = BuildQuery(warehouseId: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — DTO FIELD MAPPING
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldMapAllDtoFields_Correctly()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var expiry = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var createdAt = DateTimeOffset.UtcNow.AddDays(-10);
        var updatedAt = DateTimeOffset.UtcNow.AddDays(-1);

        var item = new WarehouseItemEntity
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouseId,
            IngredientId = ingredientId,
            Unit = "kg",
            Quantity = 7.5m,
            BasePrice = 12.99m,
            ExpiryDate = expiry,
            IsDeleted = false,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
        SetupQueryable(new List<WarehouseItemEntity> { item });

        var query = BuildQuery(warehouseId: warehouseId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        var dto = result.Items[0];
        dto.Id.Should().Be(item.Id);
        dto.WarehouseId.Should().Be(warehouseId);
        dto.IngredientId.Should().Be(ingredientId);
        dto.Unit.Should().Be("kg");
        dto.Quantity.Should().Be(7.5m);
        dto.BasePrice.Should().Be(12.99m);
        dto.ExpiryDate.Should().Be(expiry);
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    private void SetupQueryable(List<WarehouseItemEntity> items)
    {
        var mock = items.BuildMock<WarehouseItemEntity>();
        _itemRepositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(mock);
    }

    private static GetWarehouseItemsQuery BuildQuery(
        Guid warehouseId = default,
        Guid? ingredientId = null,
        bool? lowStock = null,
        bool? expiringSoon = null,
        int page = 1,
        int pageSize = 10
    ) => new GetWarehouseItemsQuery(
        WarehouseId: warehouseId == default ? Guid.NewGuid() : warehouseId,
        IngredientId: ingredientId,
        LowStock: lowStock,
        ExpiringSoon: expiringSoon,
        Page: page,
        PageSize: pageSize
    );

    private static WarehouseItemEntity BuildItem(
        Guid? warehouseId = null,
        Guid? ingredientId = null,
        decimal quantity = 20m,
        DateTime? expiryDate = null
    ) => new()
    {
        Id = Guid.NewGuid(),
        WarehouseId = warehouseId ?? Guid.NewGuid(),
        IngredientId = ingredientId ?? Guid.NewGuid(),
        Unit = "kg",
        Quantity = quantity,
        BasePrice = 10m,
        ExpiryDate = expiryDate,
        IsDeleted = false,
        CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
        UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
    };
}