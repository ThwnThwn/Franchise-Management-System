using InventoryService.Application.ClientInterface;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Features.StoreIngredient.Commands.SyncZeroCostIngredients
{
    public class SyncZeroCostIngredientsHandler : IRequestHandler<SyncZeroCostIngredientsCommand, SyncZeroCostIngredientsResult>
    {
        private readonly IIngredientRepository _ingredientRepo; // Dùng Repo thay cho DbContext
        private readonly ISupplierServiceClient _supplierServiceClient;
        private readonly ILogger<SyncZeroCostIngredientsHandler> _logger;
        private readonly IMediator _mediator;

        public SyncZeroCostIngredientsHandler(
            IIngredientRepository ingredientRepo,
            ISupplierServiceClient supplierServiceClient,
            ILogger<SyncZeroCostIngredientsHandler> logger,
            IMediator mediator)
        {
            _ingredientRepo = ingredientRepo;
            _supplierServiceClient = supplierServiceClient;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<SyncZeroCostIngredientsResult> Handle(SyncZeroCostIngredientsCommand request, CancellationToken cancellationToken)
        {
            // 1. Lấy danh sách qua Repository
            var zeroCostIngredients = await _ingredientRepo.GetZeroCostIngredientsAsync(cancellationToken);

            if (zeroCostIngredients == null || zeroCostIngredients.Count == 0)
            {
                return new SyncZeroCostIngredientsResult { UpdatedCount = 0, Message = "Không có nguyên liệu nào có giá trị cost = 0." };
            }

            var ingredientIds = zeroCostIngredients.Select(i => i.Id).ToList();

            // 2. Lấy giá từ SupplierService (gRPC)
            Dictionary<Guid, decimal> latestPrices;
            try
            {
                latestPrices = await _supplierServiceClient.GetLatestPriceForIngredientsAsync(ingredientIds, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call SupplierService for syncing zero cost ingredients.");
                return new SyncZeroCostIngredientsResult { UpdatedCount = 0, Message = "Đã xảy ra lỗi khi gọi qua SupplierService để lấy giá." };
            }

            // 3. Cập nhật giá nội bộ
            int updatedCount = 0;
            foreach (var ingredient in zeroCostIngredients)
            {
                if (latestPrices.TryGetValue(ingredient.Id, out var newCost) && newCost > 0)
                {
                    ingredient.Cost = newCost;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await _ingredientRepo.SaveChangesAsync(cancellationToken);
            }

            // 4. Gọi Mediator tái tính toán giá vốn cho các món ăn (Variant) liên quan
            foreach (var ingredient in zeroCostIngredients)
            {
                if (latestPrices.TryGetValue(ingredient.Id, out var newCost) && newCost > 0)
                {
                    try
                    {
                        var cmd = new InventoryService.Application.Features.StoreIngredient.Commands.RecalculateVariantCosts.RecalculateVariantCostsByIngredientCommand
                        {
                            IngredientId = ingredient.Id
                        };
                        await _mediator.Send(cmd, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error recalculating variant costs for ingredient {Id}", ingredient.Id);
                    }
                }
            }

            return new SyncZeroCostIngredientsResult { UpdatedCount = updatedCount, Message = $"Đã đồng bộ {updatedCount} nguyên liệu." };
        }
    }
}