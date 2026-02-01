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

namespace InventoryService.Application.Features.StoreIngredient.Commands.DecreaseStockByOrder
{
    public class DecreaseStockByOrderCommandHandler : IRequestHandler<DecreaseStockByOrderCommand, Unit>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IIdentityServiceClient _identityServiceClient;
        public DecreaseStockByOrderCommandHandler(IStoreIngredientRepository storeIngredientRepo, IRecipeRepository recipeRepository, IIdentityServiceClient identityServiceClient)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _recipeRepository = recipeRepository;
            _identityServiceClient = identityServiceClient;
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
                throw new BusinessException(Message.Recipe.NotFound);
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
                    throw new BusinessException(Message.StoreIngredient.NotFoundIngredient);
                }

                if (stock.Quantity < required.Value)
                {
                    throw new BusinessException(Message.StoreIngredient.NotEnoughStock);
                }

                // Trừ kho
                stock.Quantity -= required.Value;
                stock.UpdatedAt = DateTime.UtcNow;

                if(stock.Quantity < stock.MinThreshold)
                {
                    var manager = await _identityServiceClient.GetManager(new Dtos.User.GetManagerReq{ StoreId = stock.StoreId});
                    //gòi gửi mail ii

                }
            }

            // 6. Lưu thay đổi xuống Database 1 lần duy nhất
            await _storeIngredientRepo.UpdateRangeAsync(storeStocks);

            return Unit.Value;
        }
    }
}
