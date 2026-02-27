using InventoryService.Application.Abstractions;
using InventoryService.Application.ClientInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Features.StoreIngredient.Commands.SyncZeroCostIngredients
{
    public class SyncZeroCostIngredientsHandler : IRequestHandler<SyncZeroCostIngredientsCommand, SyncZeroCostIngredientsResult>
    {
        private readonly IInventoryDbContext _dbContext;
        private readonly ISupplierServiceClient _supplierServiceClient;
        private readonly ILogger<SyncZeroCostIngredientsHandler> _logger;

        public SyncZeroCostIngredientsHandler(
            IInventoryDbContext dbContext,
            ISupplierServiceClient supplierServiceClient,
            ILogger<SyncZeroCostIngredientsHandler> logger)
        {
            _dbContext = dbContext;
            _supplierServiceClient = supplierServiceClient;
            _logger = logger;
        }

        public async Task<SyncZeroCostIngredientsResult> Handle(SyncZeroCostIngredientsCommand request, CancellationToken cancellationToken)
        {
            // Tìm tất cả Ingredients có cost = 0 (trừ các ingredients bị xóa)
            var zeroCostIngredients = await _dbContext.Ingredients
                .Where(i => i.Cost == 0 && i.IsDeleted == false)
                .ToListAsync(cancellationToken);

            if (!zeroCostIngredients.Any())
            {
                return new SyncZeroCostIngredientsResult
                {
                    UpdatedCount = 0,
                    Message = "Không có nguyên liệu nào có giá trị cost = 0."
                };
            }

            var ingredientIds = zeroCostIngredients.Select(i => i.Id).ToList();

            // Lấy giá trị từ SupplierService
            Dictionary<Guid, decimal> latestPrices = new();
            try
            {
                latestPrices = await _supplierServiceClient.GetLatestPriceForIngredientsAsync(ingredientIds, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call SupplierService for syncing zero cost ingredients.");
                return new SyncZeroCostIngredientsResult
                {
                    UpdatedCount = 0,
                    Message = "Đã xảy ra lỗi khi gọi qua SupplierService để lấy giá."
                };
            }

            int updatedCount = 0;

            foreach (var ingredient in zeroCostIngredients)
            {
                if (latestPrices.TryGetValue(ingredient.Id, out var newCost) && newCost > 0)
                {
                    ingredient.Cost = newCost;
                    updatedCount++;
                    _logger.LogInformation("Syced new cost for Ingredient {IngredientId}: 0 -> {NewCost}", ingredient.Id, newCost);
                }
            }

            if (updatedCount > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return new SyncZeroCostIngredientsResult
            {
                UpdatedCount = updatedCount,
                Message = $"Đã đồng bộ {updatedCount} nguyên liệu cũ lên giá mới từ SupplierService."
            };
        }
    }
}
