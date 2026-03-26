using System.Text.Json;
using InventoryService.Application.Abstractions;
using InventoryService.Application.AppExceptions;
using InventoryService.Application.Constants;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Application.Signals;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.Extensions;

namespace InventoryService.Application.Features.StoreIngredient.Commands.DecreaseStockByOrder
{
    /// <summary>
    /// Xử lý yêu cầu trừ tồn kho khi có đơn hàng mới (Atomic Transaction).
    /// </summary>
    public class DecreaseStockByOrderCommandHandler : IRequestHandler<DecreaseStockByOrderCommand, Unit>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IOutboxSignal _outboxSignal;
        private readonly ILogger<DecreaseStockByOrderCommandHandler> _logger;
        private readonly IInventoryDbContext _dbContext;
        private readonly IInventoryDomainService _domainService;

        public DecreaseStockByOrderCommandHandler(
            IStoreIngredientRepository storeIngredientRepo,
            IRecipeRepository recipeRepository,
            IOutboxRepository outboxRepository,
            IOutboxSignal outboxSignal,
            ILogger<DecreaseStockByOrderCommandHandler> logger,
            IInventoryDbContext dbContext,
            IInventoryDomainService domainService)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _recipeRepository = recipeRepository;
            _outboxRepository = outboxRepository;
            _outboxSignal = outboxSignal;
            _logger = logger;
            _dbContext = dbContext;
            _domainService = domainService;
        }

        /// <summary>
        /// Thực hiện trừ kho: Tính toán nguyên liệu (Domain Service) -> Transaction -> Trừ Atomic -> Kiểm tra Low Stock -> Lưu Outbox.
        /// </summary>
        public async Task<Unit> Handle(DecreaseStockByOrderCommand common, CancellationToken cancellationToken)
        {
            var req = common.Request;
            if (req.Items == null || !req.Items.Any()) return Unit.Value;

            // 1. Tính toán lượng nguyên liệu cần thiết dựa trên công thức (Sử dụng Domain Service)
            var variantIds = req.Items.Select(x => x.VariantId).Distinct().ToList();
            var allRecipes = await _recipeRepository.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);
            
            if (allRecipes == null || !allRecipes.Any())
            {
                throw new BusinessException(Message.Recipe.NotFound);
            }

            // Gọi Domain Service để tính toán
            var totalRequiredIngredients = _domainService.CalculateRequiredIngredients(
                req.Items.Select(x => (x.VariantId, x.Amount)), 
                allRecipes);

            // 2. Tải thông tin tồn kho (để kiểm tra tên và validate sơ bộ)
            var ingredientIds = totalRequiredIngredients.Keys.ToList();
            var storeStocks = await _storeIngredientRepo.GetStockByStoreIdAndIngredientIdsAsync(req.StoreId, ingredientIds, cancellationToken);

            // Validate: Đảm bảo cửa hàng có đủ các mã nguyên liệu
            if (storeStocks.Count() != ingredientIds.Count)
            {
                var missing = ingredientIds.Except(storeStocks.Select(s => s.IngredientId));
                throw new BusinessException($"Cửa hàng không có các nguyên liệu: {string.Join(", ", missing)}");
            }

            // 3. TRANSACTION CẬP NHẬT NGUYÊN TỬ (ATOMIC)
            using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var required in totalRequiredIngredients)
                {
                    var ingredientId = required.Key;
                    var amount = required.Value;
                    var stockInfo = storeStocks.FirstOrDefault(s => s.IngredientId == ingredientId);

                    // Cập nhật Atomic trực tiếp xuống DB
                    var result = await _storeIngredientRepo.DecreaseStockAtomicallyAsync(req.StoreId, ingredientId, amount);

                    if (result == null)
                    {
                        // Cập nhật thất bại -> Không đủ tồn kho
                        throw new BusinessException($"Không đủ tồn kho cho nguyên liệu {stockInfo?.Ingredient?.Name ?? ingredientId.ToString()}. Cần: {amount}");
                    }

                    _logger.LogInformation("Updated stock for Ingredient {IngredientId}. New Quantity: {Quantity}", ingredientId, result.Quantity);

                    // 4. Kiểm tra mức tồn kho thấp (Low Stock) bằng Domain Service
                    if (_domainService.IsLowStock(result.Quantity, result.MinThreshold ?? 0))
                    {
                        _logger.LogInformation("Low stock detected for Ingredient {IngredientId}. Adding to Outbox...", ingredientId);

                        var lowStockPayload = new
                        {
                            StoreId = req.StoreId,
                            IngredientId = ingredientId,
                            IngredientName = stockInfo?.Ingredient?.Name ?? "Unknown",
                            CurrentStock = result.Quantity,
                            MinThreshold = result.MinThreshold ?? 0
                        };

                        // Sử dụng Extension Method để thêm vào Outbox
                        _outboxRepository.AddEvent(lowStockPayload, "LowStockDetected");
                    }
                }

                // 5. Lưu thay đổi và Commit Transaction
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            // 6. Bắn tín hiệu cho Worker
            _outboxSignal.Pulse();

            return Unit.Value;
        }
    }
}
