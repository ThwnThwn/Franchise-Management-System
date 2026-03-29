namespace WarehouseService.Application.Tests.Features.StockTransactions;

using FluentAssertions;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.StockTransactions.Command.CreateStockIn;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class CreateStockInTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly Mock<IStockTransactionRepository> _stockTransactionRepositoryMock;
    private readonly CreateStockInCommandHandler _handler;

    public CreateStockInTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _stockTransactionRepositoryMock = new Mock<IStockTransactionRepository>();

        _handler = new CreateStockInCommandHandler(
            _warehouseRepositoryMock.Object,
            _warehouseItemRepositoryMock.Object,
            _stockTransactionRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenWarehouseDoesNotExist()
    {
        var warehouseId = Guid.NewGuid();
        var command = CreateValidCommand(warehouseId);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse '{warehouseId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenRequestContainsDuplicateIngredients()
    {
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var items = new List<StockInItemDto>
        {
            new(ingredientId, 10, 150000),
            new(ingredientId, 5, 155000)
        };

        var command = new CreateStockInCommand(warehouseId, items, Guid.NewGuid(), "PO-COFFEE-001", "Nhập gấp cà phê", Guid.NewGuid());

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId });

        // Cần BuildMock để vượt qua lần lặp đầu tiên trước khi check Duplicate ở lần 2
        var mockItems = new List<WarehouseItem>
        {
            new() { WarehouseId = warehouseId, IngredientId = ingredientId }
        }.BuildMock();

        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(mockItems);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Duplicate ingredient ID '{ingredientId}' in request.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIngredientDoesNotExistInWarehouse()
    {
        var warehouseId = Guid.NewGuid();
        var arabicaId = Guid.NewGuid();
        var command = CreateValidCommand(warehouseId, arabicaId);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId });

        // Fix: .AsQueryable() -> .BuildMock()
        var emptyItems = new List<WarehouseItem>().BuildMock();
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(emptyItems);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Ingredient '{arabicaId}' does not exist in warehouse '{warehouseId}'.*");
    }

    [Fact]
    public async Task Handle_ShouldCreatePendingTransactions_WhenRequestIsValid()
    {
        var warehouseId = Guid.NewGuid();
        var arabicaId = Guid.NewGuid();
        var robustaId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var items = new List<StockInItemDto>
        {
            new(arabicaId, 20, 160000),
            new(robustaId, 50, 90000)
        };

        var command = new CreateStockInCommand(warehouseId, items, Guid.NewGuid(), "PO-CUA-HANG-01", "Nhập kho hạt đầu tháng", userId);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId, Name = "Kho Trung Tâm Cà Phê" });

        var warehouseArabica = new WarehouseItem { Id = Guid.NewGuid(), WarehouseId = warehouseId, IngredientId = arabicaId };
        var warehouseRobusta = new WarehouseItem { Id = Guid.NewGuid(), WarehouseId = warehouseId, IngredientId = robustaId };

        // Fix: .AsQueryable() -> .BuildMock()
        var existingWarehouseItems = new List<WarehouseItem> { warehouseArabica, warehouseRobusta }.BuildMock();
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(existingWarehouseItems);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalItems.Should().Be(2);
        result.TotalAmount.Should().Be(20 * 160000 + 50 * 90000);
        result.Transactions.Should().AllSatisfy(t => t.Status.Should().Be(TransactionStatus.Pending));

        _stockTransactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockTransaction>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _stockTransactionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateStockInCommand CreateValidCommand(Guid warehouseId, Guid? ingredientId = null)
    {
        return new CreateStockInCommand(
            warehouseId,
            [new(ingredientId ?? Guid.NewGuid(), 10, 150000)],
            Guid.NewGuid(),
            "REF-001",
            "Ghi chú Arabica",
            Guid.NewGuid()
        );
    }
}