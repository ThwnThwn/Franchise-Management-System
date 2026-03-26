using FluentAssertions;
using InventoryService.Application.Abstractions;
using InventoryService.Application.Features.StoreIngredient.Commands.CaculateCostIngredientInStock;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class CaculateCostIngredientInStockHandlerTests
{
    private readonly Mock<IInventoryDbContext> _dbContextMock;
    private readonly Mock<ILogger<CaculateCostIngredientInStockHandler>> _loggerMock;
    private readonly CaculateCostIngredientInStockHandler _handler;

    public CaculateCostIngredientInStockHandlerTests()
    {
        _dbContextMock = new Mock<IInventoryDbContext>();
        _loggerMock = new Mock<ILogger<CaculateCostIngredientInStockHandler>>();
        _handler = new CaculateCostIngredientInStockHandler(_dbContextMock.Object, _loggerMock.Object);
    }

    // --- CASE 1: Ingredient không tồn tại (Branch: ?? throw KeyNotFoundException) ---
    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIngredientDoesNotExist()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        // Sửa lỗi CS1503: Dùng BuildMockDbSet và Returns(.Object)
        var ingredientsMock = new List<Ingredient>().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Ingredients).Returns(ingredientsMock.Object);

        var command = new CaculateCostIngredientInStockCommand { IngredientId = ingredientId };

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    // --- CASE 2: Tồn kho trước khi nhập bị âm (Branch: if (totalStockBefore < 0)) ---
    [Fact]
    public async Task Handle_ShouldSetStockBeforeToZero_WhenTotalStockAfterIsLessThanAddedQuantity()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var ingredient = new Ingredient { Id = ingredientId, Name = "Cafe", Cost = 100, IsDeleted = false };
        var ingredientsMock = new List<Ingredient> { ingredient }.BuildMockDbSet();

        // Giả lập: Stock After (5) - Added (10) = -5 => Phải đưa về 0
        var storeIngredientsMock = new List<Domain.Entities.StoreIngredient>
        {
            new() { IngredientId = ingredientId, Quantity = 5, IsDeleted = false }
        }.BuildMockDbSet();

        _dbContextMock.Setup(x => x.Ingredients).Returns(ingredientsMock.Object);
        _dbContextMock.Setup(x => x.StoreIngredients).Returns(storeIngredientsMock.Object);

        var command = new CaculateCostIngredientInStockCommand
        {
            IngredientId = ingredientId,
            AddedQuantity = 10,
            UnitPrice = 200
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalStockBefore.Should().Be(0);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- CASE 3 & 4: Tổng tồn sau khi nhập = 0 (Branch: if (totalStockAfter <= 0)) ---
    [Theory]
    [InlineData(100, 100)] // UnitPrice > 0 => NewCost = 100 (Lấy UnitPrice)
    [InlineData(0, 50)]   // UnitPrice <= 0 => NewCost = 50 (Lấy OldCost)
    public async Task Handle_ShouldHandleZeroTotalStockAfter_Correctly(decimal unitPrice, decimal expectedCost)
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var ingredient = new Ingredient { Id = ingredientId, Cost = 50, IsDeleted = false };
        var ingredientsMock = new List<Ingredient> { ingredient }.BuildMockDbSet();
        var storeIngredientsMock = new List<Domain.Entities.StoreIngredient>().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Ingredients).Returns(ingredientsMock.Object);
        _dbContextMock.Setup(x => x.StoreIngredients).Returns(storeIngredientsMock.Object);

        var command = new CaculateCostIngredientInStockCommand { IngredientId = ingredientId, AddedQuantity = 0, UnitPrice = unitPrice };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.NewCost.Should().Be(expectedCost);
    }

    // --- CASE 5 & 6: Giá cũ = 0 và Không nhập thêm (Branch: else if (oldCost == 0 && request.AddedQuantity == 0)) ---
    [Theory]
    [InlineData(200, 200)] // UnitPrice > 0 => NewCost = 200
    [InlineData(0, 0)]     // UnitPrice <= 0 => NewCost = 0
    public async Task Handle_ShouldHandleZeroOldCostAndNoAddedQty_Correctly(decimal unitPrice, decimal expectedCost)
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var ingredient = new Ingredient { Id = ingredientId, Cost = 0, IsDeleted = false };
        var ingredientsMock = new List<Ingredient> { ingredient }.BuildMockDbSet();
        var storeIngredientsMock = new List<Domain.Entities.StoreIngredient>
        {
            new() { IngredientId = ingredientId, Quantity = 10, IsDeleted = false }
        }.BuildMockDbSet();

        _dbContextMock.Setup(x => x.Ingredients).Returns(ingredientsMock.Object);
        _dbContextMock.Setup(x => x.StoreIngredients).Returns(storeIngredientsMock.Object);

        var command = new CaculateCostIngredientInStockCommand { IngredientId = ingredientId, AddedQuantity = 0, UnitPrice = unitPrice };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.NewCost.Should().Be(expectedCost);
    }

    // --- CASE 7: Giá cũ = 0 nhưng có nhập thêm (Branch: else if (oldCost == 0 && request.AddedQuantity > 0)) ---
    [Fact]
    public async Task Handle_ShouldSetNewCostToUnitPrice_WhenOldCostIsZeroButAddingStock()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var ingredient = new Ingredient { Id = ingredientId, Cost = 0, IsDeleted = false };
        var ingredientsMock = new List<Ingredient> { ingredient }.BuildMockDbSet();
        var storeIngredientsMock = new List<Domain.Entities.StoreIngredient>
        {
            new() { IngredientId = ingredientId, Quantity = 10, IsDeleted = false }
        }.BuildMockDbSet();

        _dbContextMock.Setup(x => x.Ingredients).Returns(ingredientsMock.Object);
        _dbContextMock.Setup(x => x.StoreIngredients).Returns(storeIngredientsMock.Object);

        var command = new CaculateCostIngredientInStockCommand { IngredientId = ingredientId, AddedQuantity = 5, UnitPrice = 150 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.NewCost.Should().Be(150);
    }

    // --- CASE 8: Tính MAC chuẩn (Branch: else) và làm tròn 2 chữ số ---
    [Fact]
    public async Task Handle_ShouldCalculateStandardMAC_AndRoundCorrectly()
    {
        // Arrange: (10 * 100 + 5 * 120) / 15 = 106.666... -> 106.67
        var ingredientId = Guid.NewGuid();
        var ingredient = new Ingredient { Id = ingredientId, Name = "Sữa", Cost = 100, IsDeleted = false };
        var ingredientsMock = new List<Ingredient> { ingredient }.BuildMockDbSet();
        var storeIngredientsMock = new List<Domain.Entities.StoreIngredient>
        {
            new() { IngredientId = ingredientId, Quantity = 15, IsDeleted = false }
        }.BuildMockDbSet();

        _dbContextMock.Setup(x => x.Ingredients).Returns(ingredientsMock.Object);
        _dbContextMock.Setup(x => x.StoreIngredients).Returns(storeIngredientsMock.Object);

        var command = new CaculateCostIngredientInStockCommand { IngredientId = ingredientId, AddedQuantity = 5, UnitPrice = 120 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.NewCost.Should().Be(106.67m);
        result.TotalStockBefore.Should().Be(10);
        result.TotalStockAfter.Should().Be(15);
    }
}