using FluentAssertions;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Dtos.Product;
using InventoryService.Application.Features.StoreIngredient.Commands.RecalculateVariantCosts;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class RecalculateVariantCostsByIngredientHandlerTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly Mock<IProductServiceClient> _productClientMock;
    private readonly Mock<ILogger<RecalculateVariantCostsByIngredientHandler>> _loggerMock;
    private readonly RecalculateVariantCostsByIngredientHandler _handler;

    public RecalculateVariantCostsByIngredientHandlerTests()
    {
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _productClientMock = new Mock<IProductServiceClient>();
        _loggerMock = new Mock<ILogger<RecalculateVariantCostsByIngredientHandler>>();

        _handler = new RecalculateVariantCostsByIngredientHandler(
            _recipeRepoMock.Object,
            _productClientMock.Object,
            _loggerMock.Object);
    }

    // 1. Test case: Không tìm thấy Variant nào (Phủ dòng 35-41)
    [Fact]
    public async Task Handle_ShouldReturnMessage_WhenNoVariantsFound()
    {
        var ingredientId = Guid.NewGuid();
        _recipeRepoMock.Setup(x => x.GetVariantIdsByIngredientIdAsync(ingredientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid>());

        var result = await _handler.Handle(new RecalculateVariantCostsByIngredientCommand { IngredientId = ingredientId }, CancellationToken.None);

        result.Message.Should().Contain("No variants found");
    }

    // 2. Test case: Tính toán chuẩn và Update gRPC (Phủ dòng 43-100)
    [Fact]
    public async Task Handle_ShouldCalculateAndCallUpdate_WhenEverythingIsSuccess()
    {
        // Arrange
        var targetIngId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        _recipeRepoMock.Setup(x => x.GetVariantIdsByIngredientIdAsync(targetIngId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { variantId });

        // QUAN TRỌNG: Tên hàm Mock phải khớp 100% với Handler
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdWithIngredientAsync(variantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Recipe> {
                new() {
                    VariantId = variantId,
                    Quantity = 2,
                    Ingredient = new Domain.Entities.Ingredient { Cost = 10 }
                }
            });

        _productClientMock.Setup(x => x.GetVariantById(variantId))
            .ReturnsAsync(new ProductVariantRespone { Cost = 15m });

        // Act
        var result = await _handler.Handle(new RecalculateVariantCostsByIngredientCommand { IngredientId = targetIngId }, CancellationToken.None);

        // Assert
        result.Results.First().NewCost.Should().Be(20); // 2 * 10
        _productClientMock.Verify(x => x.UpdateVariantCostsAsync(It.IsAny<Dictionary<Guid, decimal>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // 3. Test case: gRPC lỗi nhưng vẫn chạy tiếp (Phủ dòng 77-80)
    [Fact]
    public async Task Handle_ShouldContinue_WhenGrpcFails()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        _recipeRepoMock.Setup(x => x.GetVariantIdsByIngredientIdAsync(targetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { variantId });

        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdWithIngredientAsync(variantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Recipe> {
                new() { VariantId = variantId, Quantity = 1, Ingredient = new Domain.Entities.Ingredient { Cost = 50 } }
            });

        // Ép lỗi gRPC
        _productClientMock.Setup(x => x.GetVariantById(variantId)).ThrowsAsync(new Exception("gRPC Down"));

        // Act
        var result = await _handler.Handle(new RecalculateVariantCostsByIngredientCommand { IngredientId = targetId }, CancellationToken.None);

        // Assert
        result.Results.First().OldCost.Should().Be(0);
        _loggerMock.Verify(l => l.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}