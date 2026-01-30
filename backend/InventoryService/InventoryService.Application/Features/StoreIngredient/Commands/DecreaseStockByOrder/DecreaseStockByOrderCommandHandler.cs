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

namespace InventoryService.Application.Features.StoreIngredient.Commands.DecreaseStockByOrder
{
    public class DecreaseStockByOrderCommandHandler : IRequestHandler<DecreaseStockByOrderCommand, Unit>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IIdentityServiceClient _identityServiceClient;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ILogger<DecreaseStockByOrderCommandHandler> _logger;
        public DecreaseStockByOrderCommandHandler(
            IStoreIngredientRepository storeIngredientRepo, 
            IRecipeRepository recipeRepository, 
            IIdentityServiceClient identityServiceClient, 
            IKafkaProducer kafkaProducer,
            ILogger<DecreaseStockByOrderCommandHandler> logger)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _recipeRepository = recipeRepository;
            _identityServiceClient = identityServiceClient;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        public async Task<Unit> Handle(DecreaseStockByOrderCommand common, CancellationToken cancellationToken)
        {
            var req = common.Request;
            if (req.Items == null || !req.Items.Any()) return Unit.Value;

            // 1. Lấy danh sách tất cả VariantId từ request
            var variantIds = req.Items.Select(x => x.VariantId).Distinct().ToList();

            // 2. Lấy toàn bộ công thức liên quan (Nên dùng hàm GetByListVariantIds)
            var allRecipes = await _recipeRepository.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);
            if (allRecipes == null || !allRecipes.Any())
            {
                throw new BusinessException(Message.Inventory.NotFound);
            }

            // 3. Tính toán tổng lượng nguyên liệu cần thiết cho cả đơn hàng
            // Dùng Dictionary để cộng dồn: Key là IngredientId, Value là tổng số lượng cần trừ
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

            // 4. Lấy tồn kho thực tế của các nguyên liệu này tại Store
            var ingredientIds = totalRequiredIngredients.Keys.ToList();
            var storeStocks = await _storeIngredientRepo.GetStockByStoreIdAndIngredientIdsAsync(req.StoreId, ingredientIds, cancellationToken);

            // 5. Kiểm tra và thực hiện trừ trên đối tượng (In-memory)
            foreach (var required in totalRequiredIngredients)
            {
                var stock = storeStocks.FirstOrDefault(s => s.IngredientId == required.Key);

                if (stock == null)
                {
                    throw new BusinessException($"Cửa hàng không có nguyên liệu ID: {required.Key} trong kho.");
                }

                if (stock.Quantity < required.Value)
                {
                    throw new BusinessException($"Không đủ nguyên liệu trong kho. Nguyên liệu ID: {required.Key}, Cần: {required.Value}, Hiện có: {stock.Quantity}");
                }

                // Trừ kho
                stock.Quantity -= required.Value;
                stock.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Updated stock for Ingredient {IngredientId}. New Quantity: {Quantity}, Threshold: {Threshold}", 
                    stock.IngredientId, stock.Quantity, stock.MinThreshold);

                if ((stock.Quantity ?? 0) < (stock.MinThreshold ?? 0))
                {
                    _logger.LogInformation("Low stock detected for Ingredient {IngredientId}. Fetching Manager...", stock.IngredientId);
                    
                    Dtos.User.UserRep? manager = null;
                    try
                    {
                        manager = await _identityServiceClient.GetManager(new Dtos.User.GetManagerReq { StoreId = stock.StoreId });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to fetch manager for Store {StoreId}: {Message}", stock.StoreId, ex.Message);
                    }
                   
                    if (manager != null)
                    {
                        var lowStockEvent = new LowStockEvent
                        {
                            StoreId = stock.StoreId,
                            IngredientId = stock.IngredientId,
                            IngredientName = stock.Ingredient.Name,
                            CurrentStock = stock.Quantity ?? 0,
                            MinThreshold = stock.MinThreshold ?? 0,
                            ManagerEmail = manager.Email 
                        };
                        
                        _logger.LogInformation("Publishing LowStockEvent to Kafka for Manager {Email}", manager.Email);
                        await _kafkaProducer.PublishAsync("low-stock-notifications", lowStockEvent);
                    }
                    else
                    {
                        _logger.LogWarning("Low stock detected but NO Manager found for Store {StoreId}", stock.StoreId);
                    }
                }
            }

            // 6. Lưu thay đổi xuống Database 1 lần duy nhất
            await _storeIngredientRepo.UpdateRangeAsync(storeStocks);

            return Unit.Value;
        }
    }
}
