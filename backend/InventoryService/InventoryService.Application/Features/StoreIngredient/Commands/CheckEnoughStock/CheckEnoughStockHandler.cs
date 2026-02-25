using InventoryService.Application.AppExceptions;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Persistences.Repositories;
using MediatR;

namespace InventoryService.Application.Features.StoreIngredient.Commands.CheckEnoughStock
{
    /// <summary>
    /// Handler kiểm tra tồn kho khả dụng cho danh sách sản phẩm.
    /// Tính Available Stock = Physical Stock - Reserved Quantity (đang giữ trong Queue).
    /// </summary>
    public class CheckEnoughStockHandler : IRequestHandler<CheckEnoughStockCommand, CheckEnoughStockRes>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        private readonly IRecipeRepository _recipeRepository;

        public CheckEnoughStockHandler(IStoreIngredientRepository storeIngredientRepo, IRecipeRepository recipeRepository)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _recipeRepository = recipeRepository;
        }

        public async Task<CheckEnoughStockRes> Handle(CheckEnoughStockCommand command, CancellationToken cancellationToken)
        {
            var response = new CheckEnoughStockRes { IsEnough = true };
            var shortageItems = new List<AmountVariant>();
            var request = command.Request;

            // 1. Lấy danh sách VariantIds từ request
            var variantIds = request.CheckItem.Select(x => x.VariantId).ToList();

            // 2. Lấy tất cả công thức liên quan
            var recipes = await _recipeRepository.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);

            if (recipes == null || !recipes.Any())
            {
                throw new BusinessException(Message.Recipe.NotFound);
            }

            // 3. Lấy tồn kho của cửa hàng cho các nguyên liệu trong công thức đó
            var ingredientIds = recipes.Select(r => r.IngredientId).Distinct().ToList();
            var stockInStore = await _storeIngredientRepo.GetStockByStoreIdAndIngredientIdsAsync(request.StoreId, ingredientIds, cancellationToken);

            //Trừ đi Reserved Quantity trước khi check
            var workingStockDict = new Dictionary<Guid, decimal>();
            foreach (var stock in stockInStore)
            {
                var reservedQty = await _storeIngredientRepo.GetReservedQuantityAsync(
                    request.StoreId, stock.IngredientId);
                workingStockDict[stock.IngredientId] = (stock.Quantity ?? 0) - reservedQty;
            }

            // 4. Kiểm tra từng Item trong yêu cầu
            foreach (var item in request.CheckItem)
            {
                var itemRecipes = recipes.Where(r => r.VariantId == item.VariantId).ToList();

                // Giả định ban đầu là có thể pha đủ số lượng yêu cầu
                decimal maxPossible = item.Amount;

                foreach (var recipe in itemRecipes)
                {
                    workingStockDict.TryGetValue(recipe.IngredientId, out decimal currentStock);

                    // Tính số lượng ly tối đa dựa trên nguyên liệu này: Tồn kho / Định lượng 1 ly
                    // Dùng Math.Floor vì không thể pha lẻ ly nếu thiếu nguyên liệu
                    decimal possibleWithThisIngredient = Math.Floor(currentStock / recipe.Quantity);

                    if (possibleWithThisIngredient < maxPossible)
                    {
                        maxPossible = possibleWithThisIngredient;
                    }
                }

                // Nếu số lượng khả thi thấp hơn yêu cầu
                if (maxPossible < item.Amount)
                {
                    response.IsEnough = false;
                    shortageItems.Add(new AmountVariant
                    {
                        VariantId = item.VariantId,
                        Sku = item.Sku,
                        Amount = (int)maxPossible // Trả về số lượng tối đa có thể pha (ví dụ: 3)
                    });
                }

                //giả định trừ kho để các sản phẩm không bị ảo nguyên liệu nếu pha chế
                foreach (var recipe in itemRecipes)
                {
                    if (workingStockDict.ContainsKey(recipe.IngredientId))
                    {
                        workingStockDict[recipe.IngredientId] -= (maxPossible * recipe.Quantity);
                    }
                }
            }

            if (!response.IsEnough)
            {
                response.ShortageItems = shortageItems;
            }

            return response;
        }
    }
}