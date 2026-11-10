namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentAssertions;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.WarehouseTransfers.Query.GetById;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class GetWarehouseTransferTests
{
    private readonly Mock<IWarehouseTransferRepository> _warehouseTransferRepositoryMock;
    private readonly Mock<IWarehouseTransferItemRepository> _warehouseTransferItemRepositoryMock;
    private readonly GetWarehouseTransferQueryHandler _handler;

    public GetWarehouseTransferTests()
    {
        _warehouseTransferRepositoryMock = new Mock<IWarehouseTransferRepository>();
        _warehouseTransferItemRepositoryMock = new Mock<IWarehouseTransferItemRepository>();
        _handler = new GetWarehouseTransferQueryHandler(
            _warehouseTransferRepositoryMock.Object,
            _warehouseTransferItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTransferWithItems_WhenIdExists()
    {
        var transferId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // Hạt Arabica Bourbon

        var transfer = new WarehouseTransfer
        {
            Id = transferId,
            TransferCode = "TRF-ARABICA-001",
            Status = TransferStatus.Approved,
            FromWarehouseId = Guid.NewGuid(),
            ToWarehouseId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            TransferItems = []
        };

        var transferItems = new List<WarehouseTransferItem>
        {
            new()
            {
                WarehouseTransferId = transferId,
                WarehouseItemId = itemId,
                IngredientId = ingredientId,
                Quantity = 10,
                UnitPrice = 250000
            }
        }.BuildMock();

        var warehouseItems = new List<WarehouseItem>
        {
            new() { Id = itemId, Unit = "Kg", StockTransactions = [] }
        }.BuildMock();

        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        _warehouseTransferItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(transferItems);
        _warehouseTransferItemRepositoryMock.Setup(x => x.GetWarehouseItemQueryable()).Returns(warehouseItems);

        var query = new GetWarehouseTransferQuery(transferId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(transferId);
        result.TransferCode.Should().Be("TRF-ARABICA-001");
        result.Items.Should().HaveCount(1);
        result.Items![0].Unit.Should().Be("Kg");
        result.Items[0].IngredientId.Should().Be(ingredientId);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIdDoesNotExist()
    {
        var transferId = Guid.NewGuid();
        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseTransfer?)null);

        var query = new GetWarehouseTransferQuery(transferId);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse transfer with ID '{transferId}' was not found.");
    }
}