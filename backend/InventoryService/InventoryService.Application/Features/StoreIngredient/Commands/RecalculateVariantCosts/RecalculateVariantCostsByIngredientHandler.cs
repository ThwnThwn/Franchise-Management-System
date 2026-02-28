using InventoryService.Application.Abstractions;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Features.StoreIngredient.Commands.RecalculateVariantCosts;

/// <summary>
/// Handler tính lại cost cho tất cả Product Variant có sử dụng 1 Ingredient.
///
/// Luồng:
///   1. Tìm tất cả VariantId trong bảng Recipe mà dùng IngredientId này
///   2. Với mỗi VariantId, lấy toàn bộ Recipe (công thức)
///   3. Tính cost = SUM(ingredient.cost * recipe.quantity) cho mỗi variant
///   4. Lấy cost cũ của variant từ ProductService (qua gRPC)
///   5. Gọi gRPC UpdateVariantCosts sang ProductService để update cost mới
/// </summary>
public class RecalculateVariantCostsByIngredientHandler : IRequestHandler<RecalculateVariantCostsByIngredientCommand, RecalculateVariantCostsResult>
{
    private readonly IInventoryDbContext _dbContext;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<RecalculateVariantCostsByIngredientHandler> _logger;

    public RecalculateVariantCostsByIngredientHandler(
        IInventoryDbContext dbContext,
        IProductServiceClient productServiceClient,
        ILogger<RecalculateVariantCostsByIngredientHandler> logger)
    {
        _dbContext = dbContext;
        _productServiceClient = productServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Đây là hàm tính toán chính, được gọi khi có 1 ingredient thay đổi cost (ví dụ sau khi tính MAC).
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public async Task<RecalculateVariantCostsResult> Handle(RecalculateVariantCostsByIngredientCommand request, CancellationToken cancellationToken)
    {
        // 1. Tìm tất cả VariantId liên quan đến IngredientId này
        var affectedVariantIds = await _dbContext.Recipes
            .Where(r => r.IngredientId == request.IngredientId)
            .Select(r => r.VariantId)
            .Distinct() // tránh trùng lặp nếu 1 variant dùng nhiều lần cùng ingredient
            .ToListAsync(cancellationToken);

        if (!affectedVariantIds.Any())
        {
            return new RecalculateVariantCostsResult
            {
                Message = $"No variants found using ingredient {request.IngredientId}."
            };
        }

        // Dùng Dictionary để lưu variantId → newCost, sau đó gọi gRPC 1 lần để update tất cả
        var variantCostUpdates = new Dictionary<Guid, decimal>();
        var resultItems = new List<VariantCostItem>();

        // 2. Mỗi variant → lấy toàn bộ recipe → tính cost
        foreach (var variantId in affectedVariantIds)
        {
            // Lấy tất cả recipe items cho variant này (kèm Ingredient để lấy Cost)
            var recipeItems = await _dbContext.Recipes
                .Include(r => r.Ingredient)
                .Where(r => r.VariantId == variantId)
                .ToListAsync(cancellationToken);

            // Cost mới = SUM(ingredient.cost * recipe.quantity)
            decimal newCost = 0;
            foreach (var recipeItem in recipeItems)
            {
                newCost += recipeItem.Ingredient.Cost * recipeItem.Quantity;
            }
            newCost = Math.Round(newCost, 2);

            // Lấy cost cũ từ ProductService
            decimal oldCost = 0;
            try
            {
                var variantInfo = await _productServiceClient.GetVariantById(variantId);
                oldCost = variantInfo.Cost;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not get old cost for variant {VariantId}: {Error}", variantId, ex.Message);
            }

            variantCostUpdates[variantId] = newCost;
            resultItems.Add(new VariantCostItem
            {
                VariantId = variantId,
                OldCost = oldCost,
                NewCost = newCost
            });

            _logger.LogInformation(
                "Variant {VariantId}: cost {OldCost} → {NewCost}",
                variantId, oldCost, newCost);
        }

        // 3. Gọi gRPC để update cost mới cho tất cả variant
        if (variantCostUpdates.Any())
        {
            await _productServiceClient.UpdateVariantCostsAsync(variantCostUpdates, cancellationToken);
        }

        return new RecalculateVariantCostsResult
        {
            Results = resultItems,
            Message = $"Updated cost for {resultItems.Count} variant(s) affected by ingredient {request.IngredientId}."
        };
    }
}
