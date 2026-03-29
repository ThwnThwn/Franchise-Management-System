using FluentAssertions;
using InventoryService.Application.AppExceptions;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Features.StoreIngredient.Commands.CheckEnoughStock;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using Moq;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class CheckEnoughStockHandlerTests
{
    private readonly Mock<IStoreIngredientRepository> _storeIngredientRepoMock;
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly CheckEnoughStockHandler _handler;

    public CheckEnoughStockHandlerTests()
    {
        _storeIngredientRepoMock = new Mock<IStoreIngredientRepository>();
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _handler = new CheckEnoughStockHandler(_storeIngredientRepoMock.Object, _recipeRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenNoRecipesFound()
    {
        var request = new CheckEnoughStockReq
        {
            CheckItem = new List<AmountVariant> { new() { VariantId = Guid.NewGuid() } }
        };
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Recipe>());

        var command = new CheckEnoughStockCommand(request);

        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnIsEnoughTrue_WhenStockIsSufficient()
    {
        var storeId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var request = new CheckEnoughStockReq
        {
            StoreId = storeId,
            CheckItem = new List<AmountVariant> { new() { VariantId = variantId, Amount = 2, Sku = "TEST-01" } }
        };

        var recipes = new List<Domain.Entities.Recipe> { new() { VariantId = variantId, IngredientId = ingredientId, Quantity = 10 } };

        // FIX: Sử dụng StoreIngredient (Entity) thay vì StoreIngredientStock
        var stock = new List<Domain.Entities.StoreIngredient> { new() { IngredientId = ingredientId, Quantity = 100 } };

        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(recipes);
        _storeIngredientRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(storeId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock); // Moq sẽ tự hiểu Task<IEnumerable<StoreIngredient>>

        _storeIngredientRepoMock.Setup(x => x.GetReservedQuantityAsync(storeId, ingredientId)).ReturnsAsync(10);

        var result = await _handler.Handle(new CheckEnoughStockCommand(request), CancellationToken.None);

        result.IsEnough.Should().BeTrue();
        result.ShortageItems.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnIsEnoughFalse_WhenStockIsShort()
    {
        var storeId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var request = new CheckEnoughStockReq
        {
            StoreId = storeId,
            CheckItem = new List<AmountVariant> { new() { VariantId = variantId, Amount = 5, Sku = "SHORT-01" } }
        };

        var recipes = new List<Domain.Entities.Recipe> { new() { VariantId = variantId, IngredientId = ingredientId, Quantity = 10 } };
        var stock = new List<Domain.Entities.StoreIngredient> { new() { IngredientId = ingredientId, Quantity = 30 } };

        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(recipes);
        _storeIngredientRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(storeId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        _storeIngredientRepoMock.Setup(x => x.GetReservedQuantityAsync(storeId, ingredientId)).ReturnsAsync(5);

        var result = await _handler.Handle(new CheckEnoughStockCommand(request), CancellationToken.None);

        // FIX: Thêm .Should()
        result.IsEnough.Should().BeFalse();
        result.ShortageItems.Should().HaveCount(1);

        // FIX: Chuyển sang ToList() trước khi dùng index [0]
        result.ShortageItems.ToList()[0].Amount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldSimulateDeduction_WhenMultipleItemsShareIngredient()
    {
        var storeId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var variantA = Guid.NewGuid();
        var variantB = Guid.NewGuid();

        var request = new CheckEnoughStockReq
        {
            StoreId = storeId,
            CheckItem = new List<AmountVariant>
            {
                new() { VariantId = variantA, Amount = 1, Sku = "V-A" },
                new() { VariantId = variantB, Amount = 1, Sku = "V-B" }
            }
        };

        var recipes = new List<Domain.Entities.Recipe>
        {
            new() { VariantId = variantA, IngredientId = ingredientId, Quantity = 10 },
            new() { VariantId = variantB, IngredientId = ingredientId, Quantity = 10 }
        };
        var stock = new List<Domain.Entities.StoreIngredient> { new() { IngredientId = ingredientId, Quantity = 15 } };

        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(recipes);
        _storeIngredientRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(storeId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        _storeIngredientRepoMock.Setup(x => x.GetReservedQuantityAsync(storeId, ingredientId)).ReturnsAsync(0);

        var result = await _handler.Handle(new CheckEnoughStockCommand(request), CancellationToken.None);

        result.IsEnough.Should().BeFalse();
        result.ShortageItems.Should().Contain(x => x.VariantId == variantB);
        result.ShortageItems.Should().NotContain(x => x.VariantId == variantA);
    }
}