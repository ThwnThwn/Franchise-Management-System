namespace WarehouseService.Application.Tests.Features.StockTransactions;

using FluentAssertions;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.StockTransactions.Command.CreateStockOut;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class CreateStockOutTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly Mock<IStockTransactionRepository> _stockTransactionRepositoryMock;
    private readonly CreateStockOutCommandHandler _handler;

    public CreateStockOutTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _stockTransactionRepositoryMock = new Mock<IStockTransactionRepository>();

        _handler = new CreateStockOutCommandHandler(
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
        var arabicaId = Guid.NewGuid(); // Hạt Arabica Dak Lak

        var items = new List<StockOutItemDto>
        {
            new(arabicaId, 2, 180000),
            new(arabicaId, 3, 180000)
        };

        var command = new CreateStockOutCommand(warehouseId, items, "Xuất cà phê cho quầy", Guid.NewGuid());

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId });

        // Mock WarehouseItems với BuildMock để vượt qua vòng lặp đầu tiên
        var stock = new List<WarehouseItem>
        {
            new() { WarehouseId = warehouseId, IngredientId = arabicaId, Quantity = 10 }
        }.BuildMock();

        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(stock);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Duplicate ingredient ID '{arabicaId}' in request.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIngredientIsMissingFromWarehouse()
    {
        var warehouseId = Guid.NewGuid();
        var matchaId = Guid.NewGuid(); // Bột Matcha Nhật Bản
        var command = CreateValidCommand(warehouseId, matchaId);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId });

        // Sửa ở đây: .AsQueryable() -> .BuildMock()
        var emptyStock = new List<WarehouseItem>().BuildMock();
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(emptyStock);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Ingredient '{matchaId}' does not exist in warehouse '{warehouseId}'.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStockIsInsufficient()
    {
        var warehouseId = Guid.NewGuid();
        var robustaId = Guid.NewGuid(); // Hạt Robusta rang đậm
        var command = new CreateStockOutCommand(
            warehouseId,
            [new(robustaId, 10, 120000)], // Yêu cầu 10kg
            "Xuất kho Robusta",
            Guid.NewGuid());

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId });

        // Sửa ở đây: .AsQueryable() -> .BuildMock()
        var currentStock = new List<WarehouseItem>
        {
            new() { WarehouseId = warehouseId, IngredientId = robustaId, Quantity = 4 } // Chỉ còn 4kg
        }.BuildMock();

        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(currentStock);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Insufficient stock for ingredient '{robustaId}'. Available: 4, Requested: 10.");
    }

    [Fact]
    public async Task Handle_ShouldCreatePendingStockOut_WhenStockIsAvailable()
    {
        var warehouseId = Guid.NewGuid();
        var arabicaId = Guid.NewGuid();
        var condensedMilkId = Guid.NewGuid(); // Sữa đặc Ngôi Sao Phương Nam

        var items = new List<StockOutItemDto>
        {
            new(arabicaId, 5, 175000),
            new(condensedMilkId, 12, 25000)
        };

        var command = new CreateStockOutCommand(warehouseId, items, "Xuất nguyên liệu pha chế", Guid.NewGuid());

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId, Name = "Kho Phụ Liệu Quận 1" });

        // Sửa ở đây: .AsQueryable() -> .BuildMock()
        var warehouseItems = new List<WarehouseItem>
        {
            new() { Id = Guid.NewGuid(), WarehouseId = warehouseId, IngredientId = arabicaId, Quantity = 100 },
            new() { Id = Guid.NewGuid(), WarehouseId = warehouseId, IngredientId = condensedMilkId, Quantity = 48 }
        }.BuildMock();

        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalItems.Should().Be(2);
        result.Transactions.Should().AllSatisfy(t =>
        {
            t.Type.Should().Be(TransactionType.StockOut);
            t.Status.Should().Be(TransactionStatus.Pending);
        });

        _stockTransactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockTransaction>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _stockTransactionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateStockOutCommand CreateValidCommand(Guid warehouseId, Guid? ingredientId = null)
    {
        return new CreateStockOutCommand(
            warehouseId,
            Items: [new(ingredientId ?? Guid.NewGuid(), 1, 150000)],
            Note: "Ghi chú xuất kho",
            CreatedBy: Guid.NewGuid()
        );
    }
}