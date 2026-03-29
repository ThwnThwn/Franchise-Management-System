namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentAssertions;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CompleteWarehouseTransfer;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class CompleteWarehouseTransferTests
{
    private readonly Mock<IWarehouseTransferRepository> _warehouseTransferRepositoryMock;
    private readonly Mock<IWarehouseTransferItemRepository> _warehouseTransferItemRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly Mock<IStockTransactionRepository> _stockTransactionRepositoryMock;
    private readonly CompleteWarehouseTransferCommandHandler _handler;

    public CompleteWarehouseTransferTests()
    {
        _warehouseTransferRepositoryMock = new Mock<IWarehouseTransferRepository>();
        _warehouseTransferItemRepositoryMock = new Mock<IWarehouseTransferItemRepository>();
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _stockTransactionRepositoryMock = new Mock<IStockTransactionRepository>();

        _handler = new CompleteWarehouseTransferCommandHandler(
            _warehouseTransferRepositoryMock.Object,
            _warehouseTransferItemRepositoryMock.Object,
            _warehouseItemRepositoryMock.Object,
            _stockTransactionRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCompleteTransferSuccessfully_WhenAllValidationsPass()
    {
        var transferId = Guid.NewGuid();
        var fromWhId = Guid.NewGuid();
        var toWhId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // Hạt Arabica Bourbon
        var sourceItemId = Guid.NewGuid();
        var destItemId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transfer = new WarehouseTransfer
        {
            Id = transferId,
            TransferCode = "TRF-COFFEE-001",
            Status = TransferStatus.Approved,
            FromWarehouseId = fromWhId,
            ToWarehouseId = toWhId
        };

        var transferItems = new List<WarehouseTransferItem>
        {
            new() { WarehouseTransferId = transferId, WarehouseItemId = sourceItemId, IngredientId = ingredientId, Quantity = 10, UnitPrice = 200000 }
        }.BuildMock();

        var sourceItem = new WarehouseItem { Id = sourceItemId, WarehouseId = fromWhId, IngredientId = ingredientId, Quantity = 50, Unit = "Kg" };
        var destItem = new WarehouseItem { Id = destItemId, WarehouseId = toWhId, IngredientId = ingredientId, Quantity = 5, Unit = "Kg" };

        var warehouseItems = new List<WarehouseItem> { sourceItem, destItem }.BuildMock();

        _warehouseTransferRepositoryMock.Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>())).ReturnsAsync(transfer);
        _warehouseTransferItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(transferItems);
        _warehouseItemRepositoryMock.Setup(x => x.GetByIdAsync(sourceItemId, It.IsAny<CancellationToken>())).ReturnsAsync(sourceItem);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var command = new CompleteWarehouseTransferCommand(transferId, userId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(TransferStatus.Completed);
        sourceItem.Quantity.Should().Be(40);
        destItem.Quantity.Should().Be(15);
        _stockTransactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockTransaction>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _stockTransactionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransferDoesNotExist()
    {
        var transferId = Guid.NewGuid();
        _warehouseTransferRepositoryMock.Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>())).ReturnsAsync((WarehouseTransfer?)null);

        var command = new CompleteWarehouseTransferCommand(transferId, Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Warehouse transfer '{transferId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStatusIsNotApproved()
    {
        var transferId = Guid.NewGuid();
        var transfer = new WarehouseTransfer { Id = transferId, Status = TransferStatus.Pending };

        _warehouseTransferRepositoryMock.Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>())).ReturnsAsync(transfer);

        var command = new CompleteWarehouseTransferCommand(transferId, Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"Only approved transfers can be completed. Current status: {TransferStatus.Pending}.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenTransferHasNoItems()
    {
        var transferId = Guid.NewGuid();
        var transfer = new WarehouseTransfer { Id = transferId, Status = TransferStatus.Approved };

        _warehouseTransferRepositoryMock.Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>())).ReturnsAsync(transfer);
        _warehouseTransferItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(new List<WarehouseTransferItem>().BuildMock());

        var command = new CompleteWarehouseTransferCommand(transferId, Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"Transfer '{transferId}' has no items to process.");
    }

    // =========================================================================
    // NEW TEST CASE: COVER THE "fromItem is null" BRANCH
    // =========================================================================
    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenSourceItemIsMissingAtCompleteTime()
    {
        var transferId = Guid.NewGuid();
        var sourceItemId = Guid.NewGuid(); // Món hàng (ví dụ Sữa đặc) đã bị xóa khỏi DB

        var transfer = new WarehouseTransfer { Id = transferId, Status = TransferStatus.Approved };
        var transferItems = new List<WarehouseTransferItem>
        {
            new() { WarehouseTransferId = transferId, WarehouseItemId = sourceItemId, IngredientId = Guid.NewGuid(), Quantity = 50 }
        }.BuildMock();

        _warehouseTransferRepositoryMock.Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>())).ReturnsAsync(transfer);
        _warehouseTransferItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(transferItems);

        // CỐ TÌNH TRẢ VỀ NULL ĐỂ MÔ PHỎNG LỖI MẤT ITEM Ở KHO XUẤT
        _warehouseItemRepositoryMock.Setup(x => x.GetByIdAsync(sourceItemId, It.IsAny<CancellationToken>())).ReturnsAsync((WarehouseItem?)null);

        var command = new CompleteWarehouseTransferCommand(transferId, Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Source item '{sourceItemId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStockIsInsufficientAtCompleteTime()
    {
        var transferId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // Sữa đặc Larosee
        var sourceItemId = Guid.NewGuid();

        var transfer = new WarehouseTransfer { Id = transferId, Status = TransferStatus.Approved };
        var transferItems = new List<WarehouseTransferItem>
        {
            new() { WarehouseTransferId = transferId, WarehouseItemId = sourceItemId, IngredientId = ingredientId, Quantity = 100 }
        }.BuildMock();

        var sourceItem = new WarehouseItem { Id = sourceItemId, IngredientId = ingredientId, Quantity = 20 }; // Chỉ còn 20 lon

        _warehouseTransferRepositoryMock.Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>())).ReturnsAsync(transfer);
        _warehouseTransferItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(transferItems);
        _warehouseItemRepositoryMock.Setup(x => x.GetByIdAsync(sourceItemId, It.IsAny<CancellationToken>())).ReturnsAsync(sourceItem);

        var command = new CompleteWarehouseTransferCommand(transferId, Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Insufficient stock for ingredient '{ingredientId}'. Available: 20, Required: 100.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenDestinationItemIsMissing()
    {
        var transferId = Guid.NewGuid();
        var toWhId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // Bột Matcha
        var sourceItemId = Guid.NewGuid();

        var transfer = new WarehouseTransfer { Id = transferId, Status = TransferStatus.Approved, ToWarehouseId = toWhId };
        var transferItems = new List<WarehouseTransferItem>
        {
            new() { WarehouseTransferId = transferId, WarehouseItemId = sourceItemId, IngredientId = ingredientId, Quantity = 5 }
        }.BuildMock();

        var sourceItem = new WarehouseItem { Id = sourceItemId, IngredientId = ingredientId, Quantity = 50 };

        _warehouseTransferRepositoryMock.Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>())).ReturnsAsync(transfer);
        _warehouseTransferItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(transferItems);
        _warehouseItemRepositoryMock.Setup(x => x.GetByIdAsync(sourceItemId, It.IsAny<CancellationToken>())).ReturnsAsync(sourceItem);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(new List<WarehouseItem>().BuildMock());

        var command = new CompleteWarehouseTransferCommand(transferId, Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Destination warehouse '{toWhId}' has no item for ingredient '{ingredientId}'.");
    }
}