namespace WarehouseService.Application.Tests.Features.Warehouses;

using FluentAssertions;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.Warehouses.Queries.GetAll;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class GetWarehousesTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly GetWarehousesQueryHandler _handler;

    public GetWarehousesTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _handler = new GetWarehousesQueryHandler(_warehouseRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllWarehouses_WhenNoFiltersApplied()
    {
        var warehouses = new List<Warehouse>
        {
            new() { Id = Guid.NewGuid(), Code = "WH-HCM-01", Name = "Kho Cà Phê Sài Gòn", Address = "Quận 1", WarehouseItems = [], StockTransactions = [] },
            new() { Id = Guid.NewGuid(), Code = "WH-DL-02", Name = "Kho Nhân Đắk Lắk", Address = "Buôn Ma Thuột", WarehouseItems = [], StockTransactions = [] }
        }.BuildMock();

        _warehouseRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouses);

        var query = new GetWarehousesQuery(null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStoreId_WhenStoreIdIsProvided()
    {
        var storeId = Guid.NewGuid();
        var warehouses = new List<Warehouse>
        {
            new() { StoreId = storeId, Code = "WH-STORE-01", Name = "Kho Arabica", WarehouseItems = [], StockTransactions = [] },
            new() { StoreId = Guid.NewGuid(), Code = "WH-STORE-02", Name = "Kho Robusta", WarehouseItems = [], StockTransactions = [] }
        }.BuildMock();

        _warehouseRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouses);

        var query = new GetWarehousesQuery(storeId, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].StoreId.Should().Be(storeId);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus_WhenStatusIsProvided()
    {
        var warehouses = new List<Warehouse>
        {
            new() { Status = WarehouseStatus.Active, Code = "WH-ACT", Name = "Kho Đang Hoạt Động", WarehouseItems = [], StockTransactions = [] },
            new() { Status = WarehouseStatus.Inactive, Code = "WH-INA", Name = "Kho Tạm Đóng", WarehouseItems = [], StockTransactions = [] }
        }.BuildMock();

        _warehouseRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouses);

        var query = new GetWarehousesQuery(null, WarehouseStatus.Active, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be(WarehouseStatus.Active);
    }

    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm_WhenMatchesCodeNameOrAddress()
    {
        var warehouses = new List<Warehouse>
        {
            new() { Code = "WH-MOKA", Name = "Kho Moka Cầu Đất", Address = "Đà Lạt", WarehouseItems = [], StockTransactions = [] },
            new() { Code = "WH-SYN", Name = "Kho Tổng Hợp", Address = "Bình Dương", WarehouseItems = [], StockTransactions = [] }
        }.BuildMock();

        _warehouseRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouses);

        // Test tìm theo tên "Moka"
        var query = new GetWarehousesQuery(null, null, " Moka ");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Code.Should().Be("WH-MOKA");
    }

    [Fact]
    public async Task Handle_ShouldApplyPaginationCorrectly()
    {
        var warehouses = Enumerable.Range(1, 15).Select(i => new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = $"WH-BATCH-{i:00}",
            Name = $"Kho Cà Phê Batch {i}",
            Address = "Hệ thống kho",
            WarehouseItems = [],
            StockTransactions = []
        }).ToList().BuildMock();

        _warehouseRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouses);

        var query = new GetWarehousesQuery(null, null, null, Page: 2, PageSize: 5);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
        result.Page.Should().Be(2);
        result.Items[0].Code.Should().Be("WH-BATCH-06"); // Order by Code ASC
    }
}