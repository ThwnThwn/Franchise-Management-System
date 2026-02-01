using InventoryService.Application.AppExceptions;
using InventoryService.Application.ClientInterface.Indentity;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.StoreIngredient.Commands.IncreaseStock;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using InventoryService.Application.ExternalServices;
using SharedKernel.Events;
using Microsoft.Extensions.Logging;

using InventoryService.Application.Signals;
using InventoryService.Domain.Entities;
using System.Text.Json;

namespace InventoryService.Application.Features.StoreIngredient.Commands.DecreaseStockByOrder
{
    public class DecreaseStockByOrderCommandHandler : IRequestHandler<DecreaseStockByOrderCommand, Unit>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IOutboxSignal _outboxSignal;
        private readonly ILogger<DecreaseStockByOrderCommandHandler> _logger;

        public DecreaseStockByOrderCommandHandler(
            IStoreIngredientRepository storeIngredientRepo,
            IRecipeRepository recipeRepository,
            IOutboxRepository outboxRepository,
            IOutboxSignal outboxSignal,
            ILogger<DecreaseStockByOrderCommandHandler> logger)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _recipeRepository = recipeRepository;
            _outboxRepository = outboxRepository;
            _outboxSignal = outboxSignal;
            _logger = logger;
        }

        public async Task<Unit> Handle(DecreaseStockByOrderCommand common, CancellationToken cancellationToken)
        {
            var req = common.Request;
            if (req.Items == null || !req.Items.Any()) return Unit.Value;

            // 1. Lấy danh sách tất cả VariantId từ request
            var variantIds = req.Items.Select(x => x.VariantId).Distinct().ToList();

            // 2. Lấy toàn bộ công thức liên quan
            var allRecipes = await _recipeRepository.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);
            if (allRecipes == null || !allRecipes.Any())
            {
                throw new BusinessException(Message.Recipe.NotFound);
            }

            // 3. Tính toán tổng lượng nguyên liệu cần thiết
            var totalRequiredIngredients = new Dictionary<Guid, decimal>();
            foreach (var item in req.Items)
            {
                var recipesForItem = allRecipes.Where(r => r.VariantId == item.VariantId);
                foreach (var recipe in recipesForItem)
                {
                    var totalNeeded = recipe.Quantity * item.Amount;
                    if (totalRequiredIngredients.ContainsKey(recipe.IngredientId))
                    {
                        totalRequiredIngredients[recipe.IngredientId] += totalNeeded;
                    }
                    else
                    {
                        totalRequiredIngredients[recipe.IngredientId] = totalNeeded;
                    }
                }
            }

            // 4. Lấy tồn kho thực tế
            var ingredientIds = totalRequiredIngredients.Keys.ToList();
            var storeStocks = await _storeIngredientRepo.GetStockByStoreIdAndIngredientIdsAsync(req.StoreId, ingredientIds, cancellationToken);

            // 5. Kiểm tra và thực hiện trừ kho
            foreach (var required in totalRequiredIngredients)
            {
                var stock = storeStocks.FirstOrDefault(s => s.IngredientId == required.Key);

                if (stock == null)
                {
                    throw new BusinessException(Message.StoreIngredient.NotFoundIngredient);
                }

                if (stock.Quantity < required.Value)
                {
                    throw new BusinessException(Message.StoreIngredient.NotEnoughStock);
                }

                // Trừ kho
                stock.Quantity -= required.Value;
                stock.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Updated stock for Ingredient {IngredientId}. New Quantity: {Quantity}, Threshold: {Threshold}",
                    stock.IngredientId, stock.Quantity, stock.MinThreshold);

                // 6. Kiểm tra Low Stock -> Lưu vào Outbox
                // Thay vì gọi gRPC và bắn Kafka trực tiếp (gây chậm), ta lưu event vào Outbox để worker xử lý sau.
                if ((stock.Quantity ?? 0) < (stock.MinThreshold ?? 0))
                {
                    _logger.LogInformation("Low stock detected for Ingredient {IngredientId}. Adding to Outbox...", stock.IngredientId);

                    // Chỉ lưu thông tin cần thiết, Worker sẽ lo phần còn lại (Lấy Email Manager -> Bắn Kafka)
                    var lowStockPayload = new 
                    {
                        StoreId = stock.StoreId,
                        IngredientId = stock.IngredientId,
                        IngredientName = stock.Ingredient.Name,
                        CurrentStock = stock.Quantity ?? 0,
                        MinThreshold = stock.MinThreshold ?? 0
                    };

                    var outboxMessage = new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        Type = "LowStockDetected", // Internal Event Type
                        Content = JsonSerializer.Serialize(lowStockPayload),
                        OccurredOn = DateTime.UtcNow
                    };

                    _outboxRepository.Add(outboxMessage);
                }
            }

            // 7. Lưu thay đổi (Cả Stock & OutboxMessage) trong 1 Transaction (Implicit EF Core)
            await _storeIngredientRepo.UpdateRangeAsync(storeStocks);

            // 8. Bấm chuông báo Worker (Fire & Forget)
            _outboxSignal.Pulse();

            return Unit.Value;
        }
    }
}
