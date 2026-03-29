namespace WarehouseService.Application.Tests.Features.StockTransactions;

using FluentAssertions;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.StockTransactions.Query.GetById;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class GetStockTransactionTests
{
    private readonly Mock<IStockTransactionRepository> _stockTransactionRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly GetStockTransactionQueryHandler _handler;

    public GetStockTransactionTests()
    {
        _stockTransactionRepositoryMock = new Mock<IStockTransactionRepository>();
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new GetStockTransactionQueryHandler(
            _stockTransactionRepositoryMock.Object,
            _warehouseItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTransactionDetail_WhenTransactionExists()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // Hạt Arabica Bourbon Dak Lak
        var warehouseId = Guid.NewGuid();

        var transactions = new List<StockTransaction>
        {
            new()
            {
                Id = transactionId,
                TransactionCode = "ST-ARABICA-BOURBON-001",
                WarehouseId = warehouseId,
                WarehouseItemId = itemId,
                Type = TransactionType.StockIn,
                Quantity = 25,
                UnitPrice = 250000,
                TotalPrice = 6250000,
                Status = TransactionStatus.Completed,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            }
        }.BuildMock();

        var warehouseItems = new List<WarehouseItem>
        {
            new()
            {
                Id = itemId,
                IngredientId = ingredientId,
                WarehouseId = warehouseId,
                StockTransactions = [] // Rule 5: Khởi tạo collection an toàn
            }
        }.BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(transactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var query = new GetStockTransactionQuery(transactionId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(transactionId);
        result.TransactionCode.Should().Be("ST-ARABICA-BOURBON-001");
        result.IngredientId.Should().Be(ingredientId);
        result.Quantity.Should().Be(25);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransactionDoesNotExist()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var emptyTransactions = new List<StockTransaction>().BuildMock();
        var emptyItems = new List<WarehouseItem>().BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(emptyTransactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(emptyItems);

        var query = new GetStockTransactionQuery(transactionId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Stock transaction with ID '{transactionId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransactionExistsButWarehouseItemIsMissing()
    {
        // Arrange: Có giao dịch xuất kho Robusta Honey nhưng item trong kho bị mất link
        var transactionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var transactions = new List<StockTransaction>
        {
            new()
            {
                Id = transactionId,
                WarehouseItemId = itemId,
                TransactionCode = "ST-ROBUSTA-HONEY-002"
            }
        }.BuildMock();

        var emptyItems = new List<WarehouseItem>().BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(transactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(emptyItems);

        var query = new GetStockTransactionQuery(transactionId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}