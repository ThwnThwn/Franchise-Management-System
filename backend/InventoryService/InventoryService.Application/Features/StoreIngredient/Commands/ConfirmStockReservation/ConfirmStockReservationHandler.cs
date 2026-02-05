using InventoryService.Application.Abstractions;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Application.Signals;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.Extensions;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ConfirmStockReservation
{
    /// <summary>
    /// Handler xử lý xác nhận giữ hàng (Trừ kho thật).
    /// </summary>
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
        private readonly IInventoryDomainService _domainService;

        public ConfirmStockReservationHandler(
            IStoreIngredientRepository storeIngredientRepo,
            IRecipeRepository recipeRepo,
            IOutboxRepository outboxRepository,
            IOutboxSignal outboxSignal,
            IInventoryDbContext dbContext,
            ILogger<ConfirmStockReservationHandler> logger,
            IInventoryDomainService domainService)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _recipeRepo = recipeRepo;
            _outboxRepository = outboxRepository;
            _outboxSignal = outboxSignal;
            _dbContext = dbContext;
            _logger = logger;
            _domainService = domainService;
        }

        public async Task<bool> Handle(ConfirmStockReservationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Bắt đầu Confirm Stock Reservation cho Order {OrderId}", request.OrderId);

                // 1. Xóa khỏi Hàng đợi (Giải phóng giữ chỗ)
                await _storeIngredientRepo.RemoveQueueByOrderIdAsync(request.OrderId);

                // 2. Tính toán nguyên liệu thực tế cần trừ (Gom nhóm lại để tối ưu transaction)
                var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();
                var allRecipes = await _recipeRepo.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);
                
                // Sử dụng Domain Service để tính toán tổng nguyên liệu
                var totalRequiredIngredients = _domainService.CalculateRequiredIngredients(
                    request.Items.Select(x => (x.VariantId, x.Quantity)), 
                    allRecipes);

                bool lowStockDetected = false;

                // 3. Trừ tồn kho vật lý (Atomic)
                foreach (var req in totalRequiredIngredients)
                {
                    var ingredientId = req.Key;
                    var amountToDeduct = req.Value;

                    var result = await _storeIngredientRepo.DecreaseStockAtomicallyAsync(
                        request.StoreId,
                        ingredientId,
                        amountToDeduct);

                    if (result != null)
                    {
                        // 4. Check Low Stock bằng Domain Service
                        if (_domainService.IsLowStock(result.Quantity, result.MinThreshold ?? 0))
                        {
                            _logger.LogInformation("Low stock detected for Ingredient {IngredientId}. Adding to Outbox...", ingredientId);

                            var lowStockPayload = new
                            {
                                StoreId = request.StoreId,
                                IngredientId = ingredientId,
                                // IngredientName = result.Ingredient?.Name ?? "Unknown", // Có thể lấy nếu load include, tạm thời skip để tối ưu
                                CurrentStock = result.Quantity,
                                MinThreshold = result.MinThreshold ?? 0,
                            };

                            // Sử dụng Extension Method
                            _outboxRepository.AddEvent(lowStockPayload, "LowStockDetected");
                            lowStockDetected = true;
                        }
                    }
                }

                // 5. Save Outbox Messages và Pulse
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
