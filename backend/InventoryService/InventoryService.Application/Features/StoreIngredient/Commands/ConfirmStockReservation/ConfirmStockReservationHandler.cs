using System.Text.Json;
using InventoryService.Application.Abstractions;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Application.Signals;
using InventoryService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ConfirmStockReservation
{
    /// <summary>
    /// Handler xử lý xác nhận giữ hàng (Trừ kho thật).
    /// </summary>
    public class ConfirmStockReservationHandler : IRequestHandler<ConfirmStockReservationCommand, bool>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        private readonly IRecipeRepository _recipeRepo;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IOutboxSignal _outboxSignal;
        private readonly IInventoryDbContext _dbContext;
        private readonly ILogger<ConfirmStockReservationHandler> _logger;

        public ConfirmStockReservationHandler(
            IStoreIngredientRepository storeIngredientRepo,
            IRecipeRepository recipeRepo,
            IOutboxRepository outboxRepository,
            IOutboxSignal outboxSignal,
            IInventoryDbContext dbContext,
            ILogger<ConfirmStockReservationHandler> logger)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _recipeRepo = recipeRepo;
            _outboxRepository = outboxRepository;
            _outboxSignal = outboxSignal;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<bool> Handle(ConfirmStockReservationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Bắt đầu Confirm Stock Reservation cho Order {OrderId}", request.OrderId);

                // 1. Xóa khỏi Hàng đợi (Giải phóng giữ chỗ)
                await _storeIngredientRepo.RemoveQueueByOrderIdAsync(request.OrderId);

                // 2. Trừ tồn kho vật lý
                var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();
                var allRecipes = await _recipeRepo.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);

                var recipeLookup = allRecipes?
                    .GroupBy(r => r.VariantId)
                    .ToDictionary(g => g.Key, g => g.ToList())
                    ?? new Dictionary<Guid, List<Domain.Entities.Recipe>>();

                bool lowStockDetected = false;

                foreach (var item in request.Items)
                {
                    if (recipeLookup.TryGetValue(item.VariantId, out var recipes))
                    {
                        foreach (var recipe in recipes)
                        {
                            var amountToDeduct = recipe.Quantity * item.Quantity;
                            var result = await _storeIngredientRepo.DecreaseStockAtomicallyAsync(
                                request.StoreId,
                                recipe.IngredientId,
                                amountToDeduct);

                            if (result != null)
                            {
                                // Check Low Stock
                                if (result.Quantity < (result.MinThreshold ?? 0))
                                {
                                    _logger.LogInformation("Low stock detected for Ingredient {IngredientId}. Adding to Outbox...", recipe.IngredientId);

                                    var lowStockPayload = new
                                    {
                                        StoreId = request.StoreId,
                                        IngredientId = recipe.IngredientId,
                                        //IngredientName = result.Ingredient?.Name ?? "Unknown",
                                        CurrentStock = result.Quantity,
                                        MinThreshold = result.MinThreshold ?? 0,
                                    };

                                    var outboxMessage = new OutboxMessage
                                    {
                                        Id = Guid.NewGuid(),
                                        Type = "LowStockDetected",
                                        Content = JsonSerializer.Serialize(lowStockPayload),
                                        OccurredOn = DateTime.UtcNow
                                    };

                                    _outboxRepository.Add(outboxMessage);
                                    lowStockDetected = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Không tìm thấy công thức cho VariantId {VariantId}", item.VariantId);
                    }
                }

                // 3. Save Outbox Messages
                if (lowStockDetected)
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _outboxSignal.Pulse();
                }

                _logger.LogInformation("Hoàn tất Confirm Stock Reservation cho Order {OrderId}", request.OrderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Confirm Stock Reservation cho Order {OrderId}", request.OrderId);
                throw;
            }
        }
    }
}
