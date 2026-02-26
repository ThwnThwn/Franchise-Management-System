using InventoryService.Application.Abstractions;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Features.StoreIngredient.Commands.IncreaseStock
{
    /// <summary>
    /// Handler xử lý nhập kho:
    /// 1. Cộng số lượng tồn kho (StoreIngredient)
    /// </summary>
    public class IncreaseStockCommandHandler : IRequestHandler<IncreaseStockCommand, Unit>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        private readonly IIngredientRepository _ingredientRepo;
        private readonly IRecipeRepository _recipeRepo;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IInventoryDbContext _dbContext;
        private readonly ILogger<IncreaseStockCommandHandler> _logger;

        public IncreaseStockCommandHandler(
            IStoreIngredientRepository storeIngredientRepo,
            IIngredientRepository ingredientRepo,
            IRecipeRepository recipeRepo,
            IProductServiceClient productServiceClient,
            IInventoryDbContext dbContext,
            ILogger<IncreaseStockCommandHandler> logger)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _ingredientRepo = ingredientRepo;
            _recipeRepo = recipeRepo;
            _productServiceClient = productServiceClient;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Unit> Handle(IncreaseStockCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            if (request.Items == null || !request.Items.Any())
                throw new AppExceptions.BusinessException(Message.Ingredient.Required);

            // BƯỚC 1: Cộng tồn kho 
            await _storeIngredientRepo.IncreaseStockAsync(request.StoreId, request.Items);

            return Unit.Value;
        }
    }
}
