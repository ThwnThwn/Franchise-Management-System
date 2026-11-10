namespace WarehouseService.Application.Tests.Features.Warehouses;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.Warehouses.Command.DeleteWarehouse;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;

public class DeleteWarehouseTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly DeleteWarehouseCommandHandler _handler;

    public DeleteWarehouseTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _handler = new DeleteWarehouseCommandHandler(_warehouseRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenWarehouseDoesNotExist()
    {
        var warehouseId = Guid.NewGuid();
        var command = new DeleteWarehouseCommand(warehouseId);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse with ID '{warehouseId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenWarehouseStillHasStock()
    {
        var warehouseId = Guid.NewGuid();
        var command = new DeleteWarehouseCommand(warehouseId);

        // Giả lập kho vẫn còn 10kg hạt Robusta chưa sử dụng
        var warehouse = new Warehouse
        {
            Id = warehouseId,
            Name = "Kho Robusta Buôn Ma Thuột",
            WarehouseItems =
            [
                new() { IsDeleted = false, Quantity = 10, Unit = "Kg" }
            ]
        };

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        _warehouseRepositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(new List<Warehouse> { warehouse }.AsQueryable());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Warehouse cannot be deleted because it still has items in stock.");
    }

    [Fact]
    public async Task Handle_ShouldPerformSoftDelete_WhenWarehouseIsEmptyOrItemsAreDeleted()
    {
        var warehouseId = Guid.NewGuid();
        var command = new DeleteWarehouseCommand(warehouseId);

        // Giả lập kho cũ, các mặt hàng đã xóa hoặc hết sạch (Quantity = 0)
        var warehouse = new Warehouse
        {
            Id = warehouseId,
            Name = "Kho Cà Phê Thanh Lý",
            IsDeleted = false,
            WarehouseItems =
            [
                new() { IsDeleted = true, Quantity = 100 }, // Đã xóa
                new() { IsDeleted = false, Quantity = 0 }    // Hết hàng
            ]
        };

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        _warehouseRepositoryMock
            .Setup(x => x.GetQueryable())
            .Returns(new List<Warehouse> { warehouse }.AsQueryable());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().Be(warehouseId);
        result.IsDeleted.Should().BeTrue();
        warehouse.IsDeleted.Should().BeTrue();

        _warehouseRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}