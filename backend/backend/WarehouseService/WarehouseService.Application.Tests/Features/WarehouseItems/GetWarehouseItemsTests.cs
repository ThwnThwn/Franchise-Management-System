namespace WarehouseService.Application.Tests.Features.WarehouseItems;

using FluentAssertions;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.WarehouseItems.Query.GetAll;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;

public class GetWarehouseItemsTests
{
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly GetWarehouseItemsQueryHandler _handler;

    public GetWarehouseItemsTests()
    {
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new GetWarehouseItemsQueryHandler(_warehouseItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnItemsFromSpecificWarehouse_WhenNoOtherFiltersApplied()
    {
        var warehouseId = Guid.NewGuid();
        var items = new List<WarehouseItem>
        {
            new() { Id = Guid.NewGuid(), WarehouseId = warehouseId, Unit = "Kg", Quantity = 10, StockTransactions = [] },
            new() { Id = Guid.NewGuid(), WarehouseId = warehouseId, Unit = "Lon", Quantity = 5, StockTransactions = [] },
            new() { Id = Guid.NewGuid(), WarehouseId = Guid.NewGuid(), Unit = "Bao", Quantity = 100, StockTransactions = [] }
        };

        var mock = items.BuildMock();
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(mock);

        var query = new GetWarehouseItemsQuery(warehouseId, null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterLowStock_WhenLowStockIsTrue()
    {
        var warehouseId = Guid.NewGuid();
        var items = new List<WarehouseItem>
        {
            new() { WarehouseId = warehouseId, Quantity = 5, Unit = "Kg", StockTransactions = [] },
            new() { WarehouseId = warehouseId, Quantity = 50, Unit = "Kg", StockTransactions = [] }
        };

        var mock = items.BuildMock();
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(mock);

        var query = new GetWarehouseItemsQuery(warehouseId, null, true, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldFilterExpiringSoon_WhenExpiringSoonIsTrue()
    {
        var warehouseId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var items = new List<WarehouseItem>
        {
            new() { WarehouseId = warehouseId, ExpiryDate = now.AddDays(10), Unit = "Hộp", StockTransactions = [] },
            new() { WarehouseId = warehouseId, ExpiryDate = now.AddDays(60), Unit = "Túi", StockTransactions = [] }
        };

        var mock = items.BuildMock();
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(mock);

        var query = new GetWarehouseItemsQuery(warehouseId, null, null, true);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].ExpiryDate.Should().BeBefore(now.AddDays(31));
    }

    [Fact]
    public async Task Handle_ShouldFilterByIngredientId_WhenProvided()
    {
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var items = new List<WarehouseItem>
        {
            new() { WarehouseId = warehouseId, IngredientId = ingredientId, Unit = "Chai", StockTransactions = [] },
            new() { WarehouseId = warehouseId, IngredientId = Guid.NewGuid(), Unit = "Chai", StockTransactions = [] }
        };

        var mock = items.BuildMock();
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(mock);

        var query = new GetWarehouseItemsQuery(warehouseId, ingredientId, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].IngredientId.Should().Be(ingredientId);
    }

    [Fact]
    public async Task Handle_ShouldApplyPaginationAndOrderingCorrectly()
    {
        var warehouseId = Guid.NewGuid();
        var items = Enumerable.Range(1, 15).Select(i => new WarehouseItem
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouseId,
            Quantity = i,
            ExpiryDate = DateTime.UtcNow.AddMonths(i),
            Unit = "Gói",
            StockTransactions = []
        }).ToList();

        var mock = items.BuildMock();
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(mock);

        var query = new GetWarehouseItemsQuery(warehouseId, null, null, null, Page: 2, PageSize: 5);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
        result.Page.Should().Be(2);
    }
}