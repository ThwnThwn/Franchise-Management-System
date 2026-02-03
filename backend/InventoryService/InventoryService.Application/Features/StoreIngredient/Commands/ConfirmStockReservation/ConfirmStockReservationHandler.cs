using InventoryService.Application.Persistences.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ConfirmStockReservation
{
    /// <summary>
    /// Handler xử lý xác nhận giữ hàng (Trừ kho thật).
    /// </summary>
    public class ConfirmStockReservationHandler : IRequestHandler<ConfirmStockReservationCommand, bool>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        private readonly IRecipeRepository _recipeRepo;
        private readonly ILogger<ConfirmStockReservationHandler> _logger;

        public ConfirmStockReservationHandler(
            IStoreIngredientRepository storeIngredientRepo,
            IRecipeRepository recipeRepo,
            ILogger<ConfirmStockReservationHandler> logger)
        {
            _storeIngredientRepo = storeIngredientRepo;
            _recipeRepo = recipeRepo;
            _logger = logger;
        }

        public async Task<bool> Handle(ConfirmStockReservationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Bắt đầu Confirm Stock Reservation cho Order {OrderId}", request.OrderId);

                // 1. Xóa khỏi Hàng đợi (Giải phóng giữ chỗ)
                await _storeIngredientRepo.RemoveQueueByOrderIdAsync(request.OrderId);

                // 2. Trừ tồn kho vật lý
                var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();
                var allRecipes = await _recipeRepo.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);

                var recipeLookup = allRecipes?
                    .GroupBy(r => r.VariantId)
                    .ToDictionary(g => g.Key, g => g.ToList())
                    ?? new Dictionary<Guid, List<Domain.Entities.Recipe>>();

                foreach (var item in request.Items)
                {
                    if (recipeLookup.TryGetValue(item.VariantId, out var recipes))
                    {
                        foreach (var recipe in recipes)
                        {
                            var amountToDeduct = recipe.Quantity * item.Quantity;
                            await _storeIngredientRepo.DecreaseStockAtomicallyAsync(
                                request.StoreId,
                                recipe.IngredientId,
                                amountToDeduct);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Không tìm thấy công thức cho VariantId {VariantId}", item.VariantId);
                    }
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
