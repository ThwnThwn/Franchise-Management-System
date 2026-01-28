using InventoryService.Application.AppExceptions;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System.Security.AccessControl;

namespace InventoryService.Application.Features.StoreIngredient.Commands.CheckEnoughStock
{
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
            var shortageItems = new List<ShortageItem>();
            var request = command.Request;
            // 1. Lấy danh sách VariantIds từ request
            var variantIds = request.CheckItem.Select(x => x.VariantId).ToList();

            // 2. Lấy tất cả công thức liên quan
            var recipes =await _recipeRepository.GetRecipesByVariantIdsAsync(variantIds, cancellationToken)
                ?? throw new BusinessException("Không tìm thấy công thức nào cho các VariantId đã cho.");

            // 3. Lấy tồn kho của cửa hàng cho các nguyên liệu trong công thức đó
            var ingredientIds = recipes.Select(r => r.IngredientId).Distinct().ToList();
            var stockInStore = await _storeIngredientRepo.GetStockByStoreIdAndIngredientIdsAsync(request.StoreId, ingredientIds, cancellationToken);
            var stockInStoreDict = stockInStore.ToDictionary(s => s.IngredientId, s => s.Quantity);


            // 4. Kiểm tra từng Item trong yêu cầu
            foreach (var item in request.CheckItem)
            {
                var itemRecipes = recipes.Where(r => r.VariantId == item.VariantId).ToList();

                // Giả định ban đầu là có thể pha đủ số lượng yêu cầu
                decimal maxPossible = item.Amount;

                foreach (var recipe in itemRecipes)
                {
                    stockInStoreDict.TryGetValue(recipe.IngredientId, out decimal? currentStock);

                    // Tính số lượng ly tối đa dựa trên nguyên liệu này: Tồn kho / Định lượng 1 ly
                    // Dùng Math.Floor vì không thể pha lẻ ly nếu thiếu nguyên liệu
                    decimal possibleWithThisIngredient = Math.Floor((currentStock ?? 0) / recipe.Quantity);

                    if (possibleWithThisIngredient < maxPossible)
                    {
                        maxPossible = possibleWithThisIngredient;
                    }
                }

                // Nếu số lượng khả thi thấp hơn yêu cầu
                if (maxPossible < item.Amount)
                {
                    response.IsEnough = false;
                    shortageItems.Add(new ShortageItem
                    {
                        VariantId = item.VariantId,
                        MaxFeasibleQuantity = (int)maxPossible // Trả về số lượng tối đa có thể pha (ví dụ: 3)
                    });
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
