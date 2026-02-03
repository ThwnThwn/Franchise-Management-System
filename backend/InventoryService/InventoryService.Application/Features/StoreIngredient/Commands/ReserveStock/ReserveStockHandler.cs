using InventoryService.Application.Abstractions;
using InventoryService.Application.Constants;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ReserveStock;

/// <summary>
/// Handler xử lý yêu cầu giữ hàng (Reserve Stock) cho đơn hàng mới.
/// Logic: Kiểm tra tồn kho khả dụng (Thực tế - Đang giữ) >= Yêu cầu -> Thêm vào hàng đợi (Queue).
/// </summary>
public class ReserveStockHandler : IRequestHandler<ReserveStockCommand, ReserveStockResponseDto>
{
    private readonly IStoreIngredientRepository _storeIngredientRepo;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IInventoryDbContext _dbContext;

    public ReserveStockHandler(
        IStoreIngredientRepository storeIngredientRepo,
        IRecipeRepository recipeRepository,
        IInventoryDbContext dbContext)
    {
        _storeIngredientRepo = storeIngredientRepo;
        _recipeRepository = recipeRepository;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Xử lý lệnh giữ hàng.
    /// </summary>
    /// <param name="request">Thông tin yêu cầu giữ hàng (Items, StoreId, OrderId).</param>
    /// <param name="cancellationToken">Token hủy.</param>
    /// <returns>Kết quả giữ hàng (Thành công/Thất bại).</returns>
    public async Task<ReserveStockResponseDto> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
        {
            return new ReserveStockResponseDto { Success = true };
        }

        // 1. Tính toán nguyên liệu cần thiết dựa trên Công thức (Recipe)
        var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();
        var allRecipes = await _recipeRepository.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);
        
        if (allRecipes == null || !allRecipes.Any())
        {
            return new ReserveStockResponseDto { Success = false, Message = Message.Recipe.NotFound };
        }

        //Tính toán nguyên liệu cần thiết
        var totalRequiredIngredients = new Dictionary<Guid, decimal>();
        foreach (var item in request.Items)
        {
            var recipesForItem = allRecipes.Where(r => r.VariantId == item.VariantId);
            foreach (var recipe in recipesForItem)
            {
                var totalNeeded = recipe.Quantity * item.Amount;
                if (totalRequiredIngredients.ContainsKey(recipe.IngredientId))
                    totalRequiredIngredients[recipe.IngredientId] += totalNeeded;
                else
                    totalRequiredIngredients[recipe.IngredientId] = totalNeeded;
            }
        }

        var ingredientIds = totalRequiredIngredients.Keys.ToList();
        
        // 2. Tải thông tin Tồn kho Vật lý (Physical Stock) từ DB
        // Tồn kho vật lý (Physical Stock) là số lượng tồn kho thực tế trong kho
        var physicalStocks = await _storeIngredientRepo.GetStockByStoreIdAndIngredientIdsAsync(request.StoreId, ingredientIds, cancellationToken);

        // 3. Kiểm tra & Giữ hàng (Nguyên tắc: Có đủ tất cả hoặc Không gì cả - All or Nothing)
        // Cần kiểm tra: (Tồn kho thực - Đã giữ trong Queue) >= Yêu cầu
        //chỗ này có thể race condition, nhưng xác xuất rất thấp -> nếu có khả năng -> thì hoàn tiền/xinloi khách
        foreach (var req in totalRequiredIngredients)
        {
            var ingredientId = req.Key;
            var neededAmount = req.Value;
            
            var physicalItem = physicalStocks.FirstOrDefault(x => x.IngredientId == ingredientId);
            var physicalQty = physicalItem?.Quantity ?? 0;

            var currentReserved = await _storeIngredientRepo.GetReservedQuantityAsync(request.StoreId, ingredientId);
            
            var availableParams = physicalQty - currentReserved;

            if (availableParams < neededAmount)
            {
                var ingName = physicalItem?.Ingredient?.Name ?? "Unknown";
                return new ReserveStockResponseDto 
                { 
                    Success = false, 
                    Message = $"Not enough stock for {ingName}. Available: {availableParams}, Required: {neededAmount}" 
                };
            }
        }

        // 4. Chèn vào Hàng đợi (IngredientQueue)
        try 
        {
            // Tạo các item trong hàng đợi
            foreach (var req in totalRequiredIngredients)
            {
                var queueItem = new IngredientQueue
                {
                    Id = Guid.NewGuid(),
                    StoreId = request.StoreId,
                    IngredientId = req.Key,
                    OrderId = request.OrderId,
                    Amount = req.Value,
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(16), // Hết hạn sau 16 phút
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                };
                await _storeIngredientRepo.AddToQueueAsync(queueItem);
            }
            // Repo hiện tại lưu ngay lập tức (SaveChanges). 
            // Nếu muốn tối ưu batch insert thì cần refactor repo sau.
        }
        catch (Exception ex)
        {
             return new ReserveStockResponseDto { Success = false, Message = ex.Message };
        }

        return new ReserveStockResponseDto { Success = true };
    }
}
