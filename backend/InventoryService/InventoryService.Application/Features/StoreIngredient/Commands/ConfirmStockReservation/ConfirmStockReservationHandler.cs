using InventoryService.Application.Abstractions;
using InventoryService.Application.AppExceptions;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Application.Signals;
using InventoryService.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.Extensions;
using System.Linq;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ConfirmStockReservation
{
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
            // FIX L43: Dùng thuộc tính .Count thay vì hàm .Count(). 
            // Nếu request.Items là List/Collection, lỗi này sẽ biến mất hoàn toàn.
            if (request.Items == null || request.Items.Count == 0) return true;

            _logger.LogInformation("Bắt đầu Confirm Stock Reservation cho Order {OrderId}", request.OrderId);

            using (var transaction = await _dbContext.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    await _storeIngredientRepo.RemoveQueueByOrderIdAsync(request.OrderId);

                    var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();
                    var allRecipes = await _recipeRepo.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);

                    if (allRecipes == null) throw new InvalidOperationException("all recipes is null |ConfirmstockHanlder");

                    // FIX L65: Đã dùng !Any() cho IEnumerable (Recipes) - Chỗ này Duy làm đúng rồi
                    if (!allRecipes.Any()) throw new InvalidOperationException("all recipes is empty |ConfirmstockHanlder");

                    var totalRequiredIngredients = _domainService.CalculateRequiredIngredients(
                        request.Items.Select(x => (x.VariantId, x.Quantity)),
                        allRecipes);

                    bool lowStockDetected = false;

                    foreach (var ingredientEntry in totalRequiredIngredients)
                    {
                        var ingredientId = ingredientEntry.Key;
                        var result = await _storeIngredientRepo.DecreaseStockAtomicallyAsync(
                            request.StoreId, ingredientId, ingredientEntry.Value);

                        if (result == null)
                        {
                            throw new BusinessException($"Không đủ tồn kho cho nguyên liệu {ingredientId}. Cần trừ: {ingredientEntry.Value}");
                        }

                        decimal threshold = result.MinThreshold ?? 0;
                        if (_domainService.IsLowStock(result.Quantity, threshold))
                        {
                            _outboxRepository.AddEvent(new
                            {
                                StoreId = request.StoreId,
                                IngredientId = ingredientId,
                                CurrentStock = result.Quantity,
                                MinThreshold = threshold,
                            }, "LowStockDetected");
                            lowStockDetected = true;
                        }
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    if (lowStockDetected) _outboxSignal.Pulse();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Lỗi khi Confirm Stock Reservation cho Order {OrderId}", request.OrderId);
                    throw;
                }
            }
        }
    }
}