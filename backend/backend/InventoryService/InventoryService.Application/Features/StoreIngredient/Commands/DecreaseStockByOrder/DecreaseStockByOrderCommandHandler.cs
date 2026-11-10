using InventoryService.Application.Abstractions;
using InventoryService.Application.AppExceptions;
using InventoryService.Application.Constants;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Application.Signals;
using InventoryService.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.Extensions;

namespace InventoryService.Application.Features.StoreIngredient.Commands.DecreaseStockByOrder;

public class DecreaseStockByOrderCommandHandler : IRequestHandler<DecreaseStockByOrderCommand, Unit>
{
    private readonly IStoreIngredientRepository _storeIngredientRepo;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxSignal _outboxSignal;
    // FIX L22: Xóa _logger không sử dụng
    private readonly IInventoryDbContext _dbContext;
    private readonly IInventoryDomainService _domainService;

    public DecreaseStockByOrderCommandHandler(
        IStoreIngredientRepository storeIngredientRepo, IRecipeRepository recipeRepository,
        IOutboxRepository outboxRepository, IOutboxSignal outboxSignal,
        ILogger<DecreaseStockByOrderCommandHandler> logger, // Giữ logger trong constructor nếu Dependency Injection yêu cầu, nhưng không gán vào field
        IInventoryDbContext dbContext,
        IInventoryDomainService domainService)
    {
        _storeIngredientRepo = storeIngredientRepo;
        _recipeRepository = recipeRepository;
        _outboxRepository = outboxRepository;
        _outboxSignal = outboxSignal;
        _dbContext = dbContext;
        _domainService = domainService;
    }

    public async Task<Unit> Handle(DecreaseStockByOrderCommand common, CancellationToken cancellationToken)
    {
        var req = common.Request;
        if (req.Items == null || !req.Items.Any()) return Unit.Value;

        var variantIds = req.Items.Select(x => x.VariantId).Distinct().ToList();
        var allRecipes = await _recipeRepository.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);

        // FIX L53: Dùng !Any() thay vì Count() == 0
        if (allRecipes == null || !allRecipes.Any()) throw new BusinessException(Message.Recipe.NotFound);

        var totalRequiredIngredients = _domainService.CalculateRequiredIngredients(
            req.Items.Select(x => (x.VariantId, x.Amount)), allRecipes);

        var ingredientIds = totalRequiredIngredients.Keys.ToList();
        var storeStocks = (await _storeIngredientRepo.GetStockByStoreIdAndIngredientIdsAsync(req.StoreId, ingredientIds, cancellationToken)).ToList();

        if (storeStocks.Count != ingredientIds.Count)
        {
            var missing = ingredientIds.Except(storeStocks.Select(s => s.IngredientId));
            throw new BusinessException($"Cửa hàng không có các nguyên liệu: {string.Join(", ", missing)}");
        }

        using (var transaction = await _dbContext.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                bool lowStockDetected = false;
                foreach (var required in totalRequiredIngredients)
                {
                    var stockInfo = storeStocks.First(s => s.IngredientId == required.Key);
                    var isLow = await ProcessIngredientStockAsync(req.StoreId, required.Key, required.Value, stockInfo);
                    if (isLow) lowStockDetected = true;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                if (lowStockDetected) _outboxSignal.Pulse();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        return Unit.Value;
    }

    private async Task<bool> ProcessIngredientStockAsync(Guid storeId, Guid ingredientId, decimal amount, InventoryService.Domain.Entities.StoreIngredient stockInfo)
    {
        var result = await _storeIngredientRepo.DecreaseStockAtomicallyAsync(storeId, ingredientId, amount);

        if (result == null)
        {
            string errorName = (stockInfo.Ingredient != null && !string.IsNullOrEmpty(stockInfo.Ingredient.Name))
                ? stockInfo.Ingredient.Name : ingredientId.ToString();
            throw new BusinessException($"Không đủ tồn kho cho nguyên liệu {errorName}. Cần: {amount}");
        }

        decimal threshold = result.MinThreshold ?? 0;
        if (_domainService.IsLowStock(result.Quantity, threshold))
        {
            string displayName = (stockInfo.Ingredient != null && !string.IsNullOrEmpty(stockInfo.Ingredient.Name))
                ? stockInfo.Ingredient.Name : "Unknown";

            _outboxRepository.AddEvent(new
            {
                StoreId = storeId,
                IngredientId = ingredientId,
                IngredientName = displayName,
                CurrentStock = result.Quantity,
                MinThreshold = threshold
            }, "LowStockDetected");
            return true;
        }
        return false;
    }
}