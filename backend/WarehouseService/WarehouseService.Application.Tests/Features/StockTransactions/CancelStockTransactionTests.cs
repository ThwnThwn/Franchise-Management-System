namespace WarehouseService.Application.Tests.Features.StockTransactions;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.StockTransactions.Command.CancelStockTransaction;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class CancelStockTransactionTests
{
    private readonly Mock<IStockTransactionRepository> _stockTransactionRepositoryMock;
    private readonly CancelStockTransactionCommandHandler _handler;

    public CancelStockTransactionTests()
    {
        _stockTransactionRepositoryMock = new Mock<IStockTransactionRepository>();
        _handler = new CancelStockTransactionCommandHandler(_stockTransactionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransactionDoesNotExist()
    {
        var transactionId = Guid.NewGuid();
        var command = new CancelStockTransactionCommand(transactionId, Guid.NewGuid());

        _stockTransactionRepositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockTransaction?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Stock transaction with ID '{transactionId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStatusIsNotPending()
    {
        var transactionId = Guid.NewGuid();
        var command = new CancelStockTransactionCommand(transactionId, Guid.NewGuid());
        var completedTransaction = CreateStockTransaction(transactionId, TransactionStatus.Completed, TransactionType.StockIn);

        _stockTransactionRepositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedTransaction);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Only pending transactions can be cancelled. Current status: {TransactionStatus.Completed}.");
    }

    [Theory]
    [InlineData(TransactionType.TransferIn)]
    [InlineData(TransactionType.TransferOut)]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenTypeIsTransferRelated(TransactionType transferType)
    {
        var transactionId = Guid.NewGuid();
        var command = new CancelStockTransactionCommand(transactionId, Guid.NewGuid());
        var transferTransaction = CreateStockTransaction(transactionId, TransactionStatus.Pending, transferType);

        _stockTransactionRepositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transferTransaction);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Transfer transactions cannot be cancelled directly. Cancel the warehouse transfer instead.");
    }

    [Fact]
    public async Task Handle_ShouldCancelTransactionSuccessfully_WhenRequestIsValid()
    {
        var transactionId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();
        var command = new CancelStockTransactionCommand(transactionId, updatedBy);
        var pendingTransaction = CreateStockTransaction(transactionId, TransactionStatus.Pending, TransactionType.StockIn);

        _stockTransactionRepositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingTransaction);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(TransactionStatus.Cancelled);
        pendingTransaction.Status.Should().Be(TransactionStatus.Cancelled);
        _stockTransactionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static StockTransaction CreateStockTransaction(Guid id, TransactionStatus status, TransactionType type)
    {
        return new StockTransaction
        {
            Id = id,
            TransactionCode = "ST-ARABICA-DAK-LAK-001",
            WarehouseId = Guid.NewGuid(),
            WarehouseItemId = Guid.NewGuid(),
            Type = type,
            Status = status,
            Quantity = 100, // 100kg hạt cà phê
            UnitPrice = 150000,
            TotalPrice = 15000000,
            Warehouse = new Warehouse
            {
                Id = Guid.NewGuid(),
                Code = "WH-SAIGON-01",
                Name = "Kho Tổng Sữa Đặc Robusta",
                WarehouseItems = [],
                StockTransactions = [],
                TransfersIn = [],
                TransfersOut = []
            },
            WarehouseItem = new WarehouseItem
            {
                Id = Guid.NewGuid(),
                IngredientId = Guid.NewGuid(),
                Unit = "Kg",
                StockTransactions = new List<WarehouseItem>().SelectMany(x => x.StockTransactions).ToList()
            }
        };
    }
}