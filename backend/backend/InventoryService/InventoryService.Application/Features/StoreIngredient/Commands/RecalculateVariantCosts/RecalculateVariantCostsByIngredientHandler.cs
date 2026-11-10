using InventoryService.Application.Abstractions;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Features.StoreIngredient.Commands.RecalculateVariantCosts;

public class RecalculateVariantCostsByIngredientHandler : IRequestHandler<RecalculateVariantCostsByIngredientCommand, RecalculateVariantCostsResult>
{
    private readonly IRecipeRepository _recipeRepo; // Thay thế DbContext bằng Repository
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<RecalculateVariantCostsByIngredientHandler> _logger;

    public RecalculateVariantCostsByIngredientHandler(
        IRecipeRepository recipeRepo,
        IProductServiceClient productServiceClient,
        ILogger<RecalculateVariantCostsByIngredientHandler> logger)
    {
        _recipeRepo = recipeRepo;
        _productServiceClient = productServiceClient;
        _logger = logger;
    }

    public async Task<RecalculateVariantCostsResult> Handle(RecalculateVariantCostsByIngredientCommand request, CancellationToken cancellationToken)
    {
        // 1. Tìm tất cả VariantId liên quan đến IngredientId này thông qua Repository
        var affectedVariantIds = await _recipeRepo.GetVariantIdsByIngredientIdAsync(request.IngredientId, cancellationToken);

        if (affectedVariantIds == null || affectedVariantIds.Count == 0)
        {
            return new RecalculateVariantCostsResult
            {
                Message = $"No variants found using ingredient {request.IngredientId}."
            };
        }

        var variantCostUpdates = new Dictionary<Guid, decimal>();
        var resultItems = new List<VariantCostItem>();

        // 2. Duyệt từng variant để tính toán giá vốn mới
        foreach (var variantId in affectedVariantIds)
        {
            var recipeItems = await _recipeRepo.GetRecipesByVariantIdWithIngredientAsync(variantId, cancellationToken);

            // Công thức tính giá vốn: $Cost = \sum (Ingredient.Cost \times Recipe.Quantity)$
            decimal newCost = 0;
            foreach (var recipeItem in recipeItems)
            {
                newCost += recipeItem.Ingredient.Cost * recipeItem.Quantity;
            }
            newCost = Math.Round(newCost, 2);

            // 3. Lấy giá cũ từ ProductService (gRPC)
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

            _logger.LogInformation("Variant {VariantId}: cost {OldCost} → {NewCost}", variantId, oldCost, newCost);
        }

        // 4. Đồng bộ giá vốn mới sang ProductService qua gRPC
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