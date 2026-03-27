using Moq;
using FluentAssertions;
using WarehouseService.Application.Features.StockTransactions.Command.CompleteStockTransaction;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using WarehouseItemEntity = WarehouseService.Domain.Entities.WarehouseItem;
using StockTransactionEntity = WarehouseService.Domain.Entities.StockTransaction;

namespace WarehouseService.Application.Tests.Features.StockTransactions;

public class CompleteStockTransactionHandlerTests
{
    private readonly Mock<IStockTransactionRepository> _transactionRepoMock;
    private readonly Mock<IWarehouseItemRepository> _itemRepoMock;
    private readonly CompleteStockTransactionCommandHandler _handler;

    public CompleteStockTransactionHandlerTests()
    {
        _transactionRepoMock = new Mock<IStockTransactionRepository>();
        _itemRepoMock = new Mock<IWarehouseItemRepository>();

        _handler = new CompleteStockTransactionCommandHandler(
            _transactionRepoMock.Object,
            _itemRepoMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransactionDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _transactionRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockTransactionEntity?)null);

        var command = new CompleteStockTransactionCommand(new[] { id }, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenTransactionTypeIsInvalid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var transaction = new StockTransactionEntity { Id = id, Type = (TransactionType)999 }; // Invalid type

        _transactionRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var command = new CompleteStockTransactionCommand(new[] { id }, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*only StockIn/StockOut can be completed*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStatusIsNotPending()
    {
        // Arrange
        var id = Guid.NewGuid();
        var transaction = new StockTransactionEntity
        {
            Id = id,
            Type = TransactionType.StockIn,
            Status = TransactionStatus.Completed // Not Pending
        };

        _transactionRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var command = new CompleteStockTransactionCommand(new[] { id }, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*is not Pending*");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenWarehouseItemDoesNotExist()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var transaction = new StockTransactionEntity
        {
            WarehouseItemId = itemId,
            Type = TransactionType.StockIn,
            Status = TransactionStatus.Pending
        };

        _transactionRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        _itemRepoMock.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseItemEntity?)null);

        var command = new CompleteStockTransactionCommand(new[] { Guid.NewGuid() }, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{itemId}*");
    }

    [Fact]
    public async Task Handle_ShouldIncreaseQuantity_WhenTypeIsStockIn()
    {
        // Arrange
        var item = new WarehouseItemEntity { Quantity = 10m };
        var transaction = new StockTransactionEntity
        {
            Type = TransactionType.StockIn,
            Status = TransactionStatus.Pending,
            Quantity = 5m,
            TotalPrice = 100m
        };

        _transactionRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);
        _itemRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var command = new CompleteStockTransactionCommand(new[] { Guid.NewGuid() }, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        item.Quantity.Should().Be(15m); // 10 + 5
        transaction.Status.Should().Be(TransactionStatus.Completed);
        result.TotalCompleted.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldDecreaseQuantity_WhenTypeIsStockOut()
    {
        // Arrange
        var item = new WarehouseItemEntity { Quantity = 10m };
        var transaction = new StockTransactionEntity
        {
            Type = TransactionType.StockOut,
            Status = TransactionStatus.Pending,
            Quantity = 4m
        };

        _transactionRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);
        _itemRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var command = new CompleteStockTransactionCommand(new[] { Guid.NewGuid() }, Guid.NewGuid());

        // Act
        await _handler.Handle(command, default);

        // Assert
        item.Quantity.Should().Be(6m); // 10 - 4
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStockOutExceedsAvailable()
    {
        // Arrange
        var item = new WarehouseItemEntity { Quantity = 3m };
        var transaction = new StockTransactionEntity
        {
            Type = TransactionType.StockOut,
            Status = TransactionStatus.Pending,
            Quantity = 10m
        };

        _transactionRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);
        _itemRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var command = new CompleteStockTransactionCommand(new[] { Guid.NewGuid() }, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient stock*");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoIdsProvided()
    {
        // Arrange
        var command = new CompleteStockTransactionCommand(new List<Guid>(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Transactions.Should().BeEmpty();
        result.TotalCompleted.Should().Be(0);
        _transactionRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldProcessMultipleTransactions_AndCalculateTotalsCorrectly()
    {
        // Arrange
        var item = new WarehouseItemEntity { Quantity = 100m };
        var t1 = new StockTransactionEntity { Type = TransactionType.StockIn, Status = TransactionStatus.Pending, Quantity = 10, TotalPrice = 50 };
        var t2 = new StockTransactionEntity { Type = TransactionType.StockIn, Status = TransactionStatus.Pending, Quantity = 20, TotalPrice = 150 };

        _transactionRepoMock.SetupSequence(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(t1)
            .ReturnsAsync(t2);

        _itemRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var command = new CompleteStockTransactionCommand(new[] { Guid.NewGuid(), Guid.NewGuid() }, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.TotalCompleted.Should().Be(2);
        result.TotalAmount.Should().Be(200m); // 50 + 150
        item.Quantity.Should().Be(130m); // 100 + 10 + 20
    }
}