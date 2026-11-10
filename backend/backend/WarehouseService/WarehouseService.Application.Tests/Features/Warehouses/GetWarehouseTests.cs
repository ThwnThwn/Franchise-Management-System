namespace WarehouseService.Application.Tests.Features.Warehouses;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.Warehouses.Queries.GetById;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class GetWarehouseTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly GetWarehouseQueryHandler _handler;

    public GetWarehouseTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _handler = new GetWarehouseQueryHandler(_warehouseRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnWarehouse_WhenIdExists()
    {
        var warehouseId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var warehouse = new Warehouse
        {
            Id = warehouseId,
            Code = "WH-ARABICA-CAUDAT",
            Name = "Kho Cà Phê Arabica Cầu Đất",
            Address = "Thôn Trường Thọ, Xã Trạm Hành, Đà Lạt",
            StoreId = storeId,
            Status = WarehouseStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            WarehouseItems = [],
            StockTransactions = []
        };

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var query = new GetWarehouseQuery(warehouseId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(warehouseId);
        result.Code.Should().Be("WH-ARABICA-CAUDAT");
        result.Name.Should().Be("Kho Cà Phê Arabica Cầu Đất");
        result.Status.Should().Be(WarehouseStatus.Active);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIdDoesNotExist()
    {
        var warehouseId = Guid.NewGuid();
        var query = new GetWarehouseQuery(warehouseId);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse with ID '{warehouseId}' was not found.");
    }
}