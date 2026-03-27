using Moq;
using FluentAssertions;
using WarehouseService.Application.Features.StockTransactions.Command.CancelStockTransaction;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using StockTransactionEntity = WarehouseService.Domain.Entities.StockTransaction;

namespace WarehouseService.Application.Tests.Features.StockTransactions;

public class CancelStockTransactionHandlerTests
{
    private readonly Mock<IStockTransactionRepository> _transactionRepoMock;
    private readonly CancelStockTransactionCommandHandler _handler;

    public CancelStockTransactionHandlerTests()
    {
        _transactionRepoMock = new Mock<IStockTransactionRepository>();
        _handler = new CancelStockTransactionCommandHandler(_transactionRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransactionDoesNotExist()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        _transactionRepoMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockTransactionEntity?)null);

        var command = new CancelStockTransactionCommand(transactionId, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{transactionId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStatusIsNotPending()
    {
        // Arrange
        var transaction = new StockTransactionEntity
        {
            Id = Guid.NewGuid(),
            Status = TransactionStatus.Completed // Not Pending
        };

        _transactionRepoMock
            .Setup(x => x.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var command = new CancelStockTransactionCommand(transaction.Id, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only pending transactions can be cancelled*");
    }

    [Theory]
    [InlineData(TransactionType.TransferIn)]
    [InlineData(TransactionType.TransferOut)]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenTransactionIsTransferType(TransactionType type)
    {
        // Arrange
        var transaction = new StockTransactionEntity
        {
            Id = Guid.NewGuid(),
            Status = TransactionStatus.Pending,
            Type = type
        };

        _transactionRepoMock
            .Setup(x => x.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var command = new CancelStockTransactionCommand(transaction.Id, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Transfer transactions cannot be cancelled directly*");
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatusToCancelled_WhenRequestIsValid()
    {
        // Arrange
        var transaction = new StockTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionCode = "TX-123",
            WarehouseId = Guid.NewGuid(),
            WarehouseItemId = Guid.NewGuid(),
            Type = TransactionType.StockIn,
            Status = TransactionStatus.Pending
        };

        _transactionRepoMock
            .Setup(x => x.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var command = new CancelStockTransactionCommand(transaction.Id, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Status.Should().Be(TransactionStatus.Cancelled);
        transaction.Status.Should().Be(TransactionStatus.Cancelled);
        transaction.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));

        _transactionRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectResult_WhenCancelledSuccessfully()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var transaction = new StockTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionCode = "SO-999",
            WarehouseId = warehouseId,
            WarehouseItemId = itemId,
            Type = TransactionType.StockOut,
            Status = TransactionStatus.Pending
        };

        _transactionRepoMock
            .Setup(x => x.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var command = new CancelStockTransactionCommand(transaction.Id, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Id.Should().Be(transaction.Id);
        result.TransactionCode.Should().Be("SO-999");
        result.WarehouseId.Should().Be(warehouseId);
        result.WarehouseItemId.Should().Be(itemId);
        result.Type.Should().Be(TransactionType.StockOut);
    }
}