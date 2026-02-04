using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Persistences.Repositories;
using MediatR;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ReleaseStock;

public sealed class ReleaseStockHandler
    : IRequestHandler<ReleaseStockCommand, ReleaseStockRes>
{
    private readonly IStoreIngredientRepository _storeIngredientRepo;

    public ReleaseStockHandler(IStoreIngredientRepository storeIngredientRepo)
    {
        _storeIngredientRepo = storeIngredientRepo;
    }

    public async Task<ReleaseStockRes> Handle(
        ReleaseStockCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // 1. Load all queue items reserved for this Order
        var queueItems = await _storeIngredientRepo
            .GetQueueByOrderIdAsync(request.OrderId, cancellationToken);

        if (queueItems == null || !queueItems.Any())
        {
            return new ReleaseStockRes
            {
                Success = false,
                Message = "No reserved stock found in queue for this order."
            };
        }

        // 2. Convert queue → IngredientStockItem list
        var restoreItems = queueItems.Select(x => new IngredientStockItem
        {
            IngredientId = x.IngredientId,
            Quantity = x.Amount
        }).ToList();

        // 3. Restore stock back into StoreIngredients
        await _storeIngredientRepo.IncreaseStockAsync(request.StoreId, restoreItems);

        // 4. Remove queue records after releasing stock
        await _storeIngredientRepo.RemoveQueueByOrderIdAsync(request.OrderId);

        return new ReleaseStockRes
        {
            Success = true,
            Message = "Stock released successfully and queue cleared."
        };
    }
}
