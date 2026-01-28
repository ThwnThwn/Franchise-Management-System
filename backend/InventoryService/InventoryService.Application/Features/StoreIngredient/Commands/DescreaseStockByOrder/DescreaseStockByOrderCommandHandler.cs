using InventoryService.Application.AppExceptions;
using InventoryService.Application.Features.StoreIngredient.Commands.IncreaseStock;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.StoreIngredient.Commands.DescreaseStockByOrder
{
    public class DescreaseStockByOrderCommandHandler : IRequestHandler<DescreaseStockByOrderCommand, Unit>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IIngredientRepository _ingredientRepository;
        public DescreaseStockByOrderCommandHandler(IStoreIngredientRepository storeIngredientRepo, IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _recipeRepository = recipeRepository;
            _ingredientRepository = ingredientRepository;
        }

        public async Task<Unit> Handle(DescreaseStockByOrderCommand common, CancellationToken cancellationToken)
        {
            var req = common.Request;

            //lấy công thức theo variantId
            var recipes = await _recipeRepository.GetRecipesByVariantIdAsync(req.VariantId, cancellationToken);
            if(recipes == null || !recipes.Any())
            {
                throw new BusinessException("Không tìm thấy công thức cho sản phẩm này.");
            }
            //lấy danh sách nguyên liệu từ công thức
            var ingredientIds = recipes.Select(r => r.IngredientId).ToList();

            //lấy kho nguyên liệu của cửa hàng
            var storeStocks = await _storeIngredientRepo.GetStockByStoreIdAndIngredientIdsAsync(req.StoreId, ingredientIds, cancellationToken);

            // 4. Kiểm tra và trừ kho
            foreach (var recipe in recipes)
            {
                var stock = storeStocks.FirstOrDefault(s => s.IngredientId == recipe.IngredientId);

                if (stock == null)
                {
                    throw new Exception($"Cửa hàng không có nguyên liệu ID: {recipe.IngredientId} trong kho.");
                }

                if (stock.Quantity < recipe.Quantity)
                {
                    throw new Exception($"Không đủ nguyên liệu trong kho. Cần: {recipe.Quantity}, Hiện có: {stock.Quantity}");
                }

                // Thực hiện trừ kho
                stock.Quantity -= recipe.Quantity;
                stock.UpdatedAt = DateTime.UtcNow;

            }

            // 5. Lưu thay đổi (Thông qua Unit of Work hoặc Repository)
            await _storeIngredientRepo.UpdateRangeAsync(storeStocks);

            return Unit.Value;
        }
    }
}
