using InventoryService.Application.Abstractions;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Features.StoreIngredient.Commands.CaculateCostIngredientInStock;
using InventoryService.Application.Features.StoreIngredient.Commands.RecalculateVariantCosts;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Features.StoreIngredient.Commands.IncreaseStock
{
    /// <summary>
    /// Handler ĐIỀU PHỐI nhập kho — không chứa logic tính toán trực tiếp.
    /// 1. Cộng tồn kho (Repository)
    /// 2. Gọi CaculateCostIngredientInStock (MediatR) → Tính MAC
    /// 3. Gọi RecalculateVariantCostsByIngredient (MediatR) → Tính cost variant
    /// </summary>
    public class IncreaseStockCommandHandler : IRequestHandler<IncreaseStockCommand, Unit>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        private readonly IMediator _mediator;
        private readonly ILogger<IncreaseStockCommandHandler> _logger;

        public IncreaseStockCommandHandler(
            IStoreIngredientRepository storeIngredientRepo,
            IMediator mediator,
            ILogger<IncreaseStockCommandHandler> logger)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Unit> Handle(IncreaseStockCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            if (request.Items == null || !request.Items.Any())
                throw new AppExceptions.BusinessException(Message.Ingredient.Required);

            // BƯỚC 1: Cộng tồn kho
            await _storeIngredientRepo.IncreaseStockAsync(request.StoreId, request.Items);

            // BƯỚC 2: Tính MAC cho từng nguyên liệu
            // → Gọi CaculateCostIngredientInStock
            var affectedIngredientIds = new HashSet<Guid>();

            foreach (var item in request.Items)
            {
                try
                {
                    var result = await _mediator.Send(new CaculateCostIngredientInStockCommand
                    {
                        IngredientId = item.IngredientId,
                        AddedQuantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }, cancellationToken);

                    affectedIngredientIds.Add(item.IngredientId);

                    _logger.LogInformation(
                        "[IncreaseStock → MAC] {Name}: {OldCost} → {NewCost}",
                        result.IngredientName, result.OldCost, result.NewCost);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating MAC for ingredient {Id}", item.IngredientId);
                }
            }

            // BƯỚC 3: Tính lại cost cho các Product Variant liên quan
            // → Gọi RecalculateVariantCostsByIngredient
            foreach (var ingredientId in affectedIngredientIds)
            {
                try
                {
                    var result = await _mediator.Send(new RecalculateVariantCostsByIngredientCommand
                    {
                        IngredientId = ingredientId
                    }, cancellationToken);

                    _logger.LogInformation(
                        "[IncreaseStock → VariantCost] {Message}", result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error recalculating variant costs for ingredient {Id}", ingredientId);
                }
            }

            return Unit.Value;
        }
    }
}
