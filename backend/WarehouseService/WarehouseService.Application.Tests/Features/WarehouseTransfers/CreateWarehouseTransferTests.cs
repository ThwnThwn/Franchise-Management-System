namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CreateWarehouseTransfer;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class CreateWarehouseTransferTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly Mock<IWarehouseTransferRepository> _warehouseTransferRepositoryMock;
    private readonly Mock<IWarehouseTransferItemRepository> _warehouseTransferItemRepositoryMock;
    private readonly CreateWarehouseTransferCommandHandler _handler;

    public CreateWarehouseTransferTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _warehouseTransferRepositoryMock = new Mock<IWarehouseTransferRepository>();
        _warehouseTransferItemRepositoryMock = new Mock<IWarehouseTransferItemRepository>();

        _handler = new CreateWarehouseTransferCommandHandler(
            _warehouseRepositoryMock.Object,
            _warehouseItemRepositoryMock.Object,
            _warehouseTransferRepositoryMock.Object,
            _warehouseTransferItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateTransferSuccessfully_WhenDataIsValid()
    {
        var fromWhId = Guid.NewGuid();
        var toWhId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // Hạt Arabica Cầu Đất

        var fromWh = new Warehouse { Id = fromWhId, Name = "Tổng Kho Arabica Đà Lạt" };
        var toWh = new Warehouse { Id = toWhId, Name = "Chi Nhánh Robusta Sài Gòn" };
        var warehouseItem = new WarehouseItem
        {
            Id = itemId,
            WarehouseId = fromWhId,
            IngredientId = ingredientId,
            Quantity = 100,
            BasePrice = 250000,
            Unit = "Kg",
            StockTransactions = []
        };

        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(fromWhId, It.IsAny<CancellationToken>())).ReturnsAsync(fromWh);
        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(toWhId, It.IsAny<CancellationToken>())).ReturnsAsync(toWh);
        _warehouseItemRepositoryMock.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>())).ReturnsAsync(warehouseItem);

        var command = new CreateWarehouseTransferCommand(
            fromWhId,
            toWhId,
            [new TransferItemDto(itemId, 20)],
            "Điều phối hạt Arabica cho mùa lễ",
            "Giao hàng bằng xe tải chuyên dụng",
            Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.TransferCode.Should().StartWith("TRF-");
        result.Status.Should().Be(TransferStatus.Pending);
        result.Items.Should().HaveCount(1);
        result.Items[0].Quantity.Should().Be(20);
        result.Items[0].UnitPrice.Should().Be(250000);

        _warehouseTransferRepositoryMock.Verify(x => x.AddAsync(It.IsAny<WarehouseTransfer>(), It.IsAny<CancellationToken>()), Times.Once);
        _warehouseTransferItemRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<WarehouseTransferItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        _warehouseTransferItemRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenSourceWarehouseDoesNotExist()
    {
        var fromWhId = Guid.NewGuid();
        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(fromWhId, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);

        var command = CreateValidCommand(fromWhId, Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Source warehouse '{fromWhId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenDestinationWarehouseDoesNotExist()
    {
        var fromWhId = Guid.NewGuid();
        var toWhId = Guid.NewGuid();
        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(fromWhId, It.IsAny<CancellationToken>())).ReturnsAsync(new Warehouse());
        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(toWhId, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);

        var command = CreateValidCommand(fromWhId, toWhId);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Destination warehouse '{toWhId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenWarehousesAreTheSame()
    {
        var whId = Guid.NewGuid();
        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(whId, It.IsAny<CancellationToken>())).ReturnsAsync(new Warehouse { Id = whId });

        var command = CreateValidCommand(whId, whId);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Source and destination warehouses must be different.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenItemDoesNotExist()
    {
        var fromWhId = Guid.NewGuid();
        var toWhId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(fromWhId, It.IsAny<CancellationToken>())).ReturnsAsync(new Warehouse { Id = fromWhId });
        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(toWhId, It.IsAny<CancellationToken>())).ReturnsAsync(new Warehouse { Id = toWhId });
        _warehouseItemRepositoryMock.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>())).ReturnsAsync((WarehouseItem?)null);

        var command = new CreateWarehouseTransferCommand(fromWhId, toWhId, [new TransferItemDto(itemId, 5)], null, null, Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse item '{itemId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenItemDoesNotBelongToSourceWarehouse()
    {
        var fromWhId = Guid.NewGuid();
        var toWhId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var wrongWhId = Guid.NewGuid(); // Kho khác (ví dụ Kho Sữa Đặc)

        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(fromWhId, It.IsAny<CancellationToken>())).ReturnsAsync(new Warehouse { Id = fromWhId });
        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(toWhId, It.IsAny<CancellationToken>())).ReturnsAsync(new Warehouse { Id = toWhId });
        _warehouseItemRepositoryMock.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WarehouseItem { Id = itemId, WarehouseId = wrongWhId });

        var command = new CreateWarehouseTransferCommand(fromWhId, toWhId, [new TransferItemDto(itemId, 5)], null, null, Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Item '{itemId}' does not belong to source warehouse.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStockIsInsufficient()
    {
        var fromWhId = Guid.NewGuid();
        var toWhId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(fromWhId, It.IsAny<CancellationToken>())).ReturnsAsync(new Warehouse { Id = fromWhId });
        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(toWhId, It.IsAny<CancellationToken>())).ReturnsAsync(new Warehouse { Id = toWhId });
        _warehouseItemRepositoryMock.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WarehouseItem { Id = itemId, WarehouseId = fromWhId, Quantity = 5 }); // Chỉ còn 5 lon sữa

        var command = new CreateWarehouseTransferCommand(fromWhId, toWhId, [new TransferItemDto(itemId, 15)], null, null, Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Insufficient stock for item '{itemId}'. Available: 5, Requested: 15.");
    }

    private static CreateWarehouseTransferCommand CreateValidCommand(Guid fromWhId, Guid toWhId)
    {
        return new CreateWarehouseTransferCommand(
            fromWhId,
            toWhId,
            [new TransferItemDto(Guid.NewGuid(), 1)],
            "Lý do chuyển hàng",
            "Ghi chú bổ sung",
            Guid.NewGuid());
    }
}