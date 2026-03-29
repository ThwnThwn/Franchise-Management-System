namespace WarehouseService.Application.Tests.Features.StockTransactions;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.StockTransactions.Command.CompleteStockTransaction;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class CompleteStockTransactionTests
{
    private readonly Mock<IStockTransactionRepository> _stockTransactionRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly CompleteStockTransactionCommandHandler _handler;

    public CompleteStockTransactionTests()
    {
        _stockTransactionRepositoryMock = new Mock<IStockTransactionRepository>();
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new CompleteStockTransactionCommandHandler(
            _stockTransactionRepositoryMock.Object,
            _warehouseItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCompleteStockInSuccessfully_WhenDataIsValid()
    {
        // Arrange: Nhập 50kg hạt Arabica Cầu Đất
        var transactionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new CompleteStockTransactionCommand([transactionId], Guid.NewGuid());

        var transaction = CreateTransaction(transactionId, itemId, TransactionType.StockIn, 50);
        var item = CreateWarehouseItem(itemId, 100); // Tồn cũ 100kg

        SetupMocks(transaction, item);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalCompleted.Should().Be(1);
        item.Quantity.Should().Be(150); // 100 + 50
        transaction.Status.Should().Be(TransactionStatus.Completed);
        _stockTransactionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCompleteStockOutSuccessfully_WhenStockIsSufficient()
    {
        // Arrange: Xuất 5kg bột Robusta pha máy
        var transactionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new CompleteStockTransactionCommand([transactionId], Guid.NewGuid());

        var transaction = CreateTransaction(transactionId, itemId, TransactionType.StockOut, 5);
        var item = CreateWarehouseItem(itemId, 10); // Tồn cũ 10kg

        SetupMocks(transaction, item);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        item.Quantity.Should().Be(5); // 10 - 5
        transaction.Status.Should().Be(TransactionStatus.Completed);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTransactionNotFound()
    {
        var transactionId = Guid.NewGuid();
        var command = new CompleteStockTransactionCommand([transactionId], Guid.NewGuid());

        _stockTransactionRepositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockTransaction?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTypeIsNotStockInOrOut()
    {
        // Case: Transaction type là Adjustment (Điều chỉnh) - không được xử lý ở đây
        var transactionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new CompleteStockTransactionCommand([transactionId], Guid.NewGuid());
        var transaction = CreateTransaction(transactionId, itemId, TransactionType.Adjustment, 10);

        SetupMocks(transaction, null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{TransactionType.Adjustment}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStatusIsNotPending()
    {
        var transactionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new CompleteStockTransactionCommand([transactionId], Guid.NewGuid());
        var transaction = CreateTransaction(transactionId, itemId, TransactionType.StockIn, 10);
        transaction.Status = TransactionStatus.Cancelled;

        SetupMocks(transaction, null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not Pending*");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenWarehouseItemNotFound()
    {
        var transactionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new CompleteStockTransactionCommand([transactionId], Guid.NewGuid());
        var transaction = CreateTransaction(transactionId, itemId, TransactionType.StockIn, 10);

        _stockTransactionRepositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);
        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseItem?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Warehouse item '{itemId}' was not found.*");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStockIsInsufficient()
    {
        // Arrange: Xuất 20kg trong khi kho chỉ còn 5kg
        var transactionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new CompleteStockTransactionCommand([transactionId], Guid.NewGuid());

        var transaction = CreateTransaction(transactionId, itemId, TransactionType.StockOut, 20);
        var item = CreateWarehouseItem(itemId, 5);

        SetupMocks(transaction, item);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient stock*");
    }

    private void SetupMocks(StockTransaction? transaction, WarehouseItem? item)
    {
        if (transaction != null)
        {
            _stockTransactionRepositoryMock
                .Setup(x => x.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);
        }

        if (item != null)
        {
            _warehouseItemRepositoryMock
                .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);
        }
    }

    private static StockTransaction CreateTransaction(Guid id, Guid itemId, TransactionType type, decimal qty)
    {
        return new StockTransaction
        {
            Id = id,
            TransactionCode = $"ST-{type.ToString().ToUpper()}-ROBUSTA-001",
            WarehouseItemId = itemId,
            WarehouseId = Guid.NewGuid(),
            Type = type,
            Status = TransactionStatus.Pending,
            Quantity = qty,
            TotalPrice = qty * 120000,
            UpdatedAt = DateTimeOffset.MinValue,
            // Khởi tạo an toàn theo Rule 5
            Warehouse = new Warehouse { Code = "KHO-01", Name = "Kho Cà Phê" },
            WarehouseItem = new WarehouseItem { Unit = "Kg" }
        };
    }

    private static WarehouseItem CreateWarehouseItem(Guid id, decimal currentQty)
    {
        return new WarehouseItem
        {
            Id = id,
            Quantity = currentQty,
            IngredientId = Guid.NewGuid(),
            Unit = "Kg",
            UpdatedAt = DateTimeOffset.MinValue,
            // Khởi tạo an toàn theo Rule 5
            StockTransactions = [],
            Warehouse = new Warehouse { Name = "Kho Quận 1" }
        };
    }
}