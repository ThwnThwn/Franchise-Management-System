using InventoryService.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Features.StoreIngredient.Commands.CaculateCostIngredientInStock;

/// <summary>
/// Handler tính MAC (Moving Average Cost) cho 1 nguyên liệu.
/// Đây là HÀM TÍNH TOÁN CHÍNH, được gọi từ 2 nơi:
///   1. IncreaseStockCommandHandler (tự động khi nhập kho)
///   2. gRPC CaculateCostIngredientInStock (test thủ công bằng Postman)
///
/// Công thức:
///   NewCost = ((TồnCũ × CostCũ) + (SốLượngNhập × GiáNhập)) / (TồnCũ + SốLượngNhập)
///
/// Lưu ý: Hàm sẽ cập nhật trực tiếp Ingredient.Cost trong DB.
/// </summary>
public class CaculateCostIngredientInStockHandler : IRequestHandler<CaculateCostIngredientInStockCommand, CaculateCostIngredientInStockResult>
{
    private readonly IInventoryDbContext _dbContext;
    private readonly ILogger<CaculateCostIngredientInStockHandler> _logger;

    public CaculateCostIngredientInStockHandler(
        IInventoryDbContext dbContext,
        ILogger<CaculateCostIngredientInStockHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CaculateCostIngredientInStockResult> Handle(
        CaculateCostIngredientInStockCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy Ingredient hiện tại
        var ingredient = await _dbContext.Ingredients
            .FirstOrDefaultAsync(i => i.Id == request.IngredientId && i.IsDeleted == false, cancellationToken)
            ?? throw new KeyNotFoundException($"Ingredient {request.IngredientId} not found.");

        var oldCost = ingredient.Cost;

        // 2. Lấy tổng tồn kho SAU KHI đã cộng (IncreaseStock đã cộng trước đó)
        var totalStockAfter = await _dbContext.StoreIngredients
            .Where(si => si.IngredientId == request.IngredientId && si.IsDeleted == false)
            .SumAsync(si => si.Quantity ?? 0, cancellationToken);

        // 3. Tồn TRƯỚC khi nhập = tồn SAU - số lượng vừa nhập
        var totalStockBefore = totalStockAfter - request.AddedQuantity;
        if (totalStockBefore < 0) totalStockBefore = 0;

        // 4. Tính MAC
        decimal newCost;
        if (totalStockAfter <= 0)
        {
            newCost = request.UnitPrice > 0 ? request.UnitPrice : oldCost;
        }
        else if (oldCost == 0 && request.AddedQuantity == 0)
        {
            // Nếu giá cũ đang bằng 0 và đang gọi tính lại (không thêm số lượng),
            // lấy UnitPrice truyền vào làm giá mới (nếu UnitPrice > 0).
            newCost = request.UnitPrice > 0 ? request.UnitPrice : 0;
        }
        else if (oldCost == 0 && request.AddedQuantity > 0)
        {
            // Nếu giá cũ đang bằng 0 mà có nhập thêm, 
            // coi như toàn bộ tồn kho hiện tại lấy theo giá mới nhập (để tránh bị chia loãng bởi giá 0)
            newCost = request.UnitPrice;
        }
        else
        {
            newCost = ((totalStockBefore * oldCost) + (request.AddedQuantity * request.UnitPrice)) / totalStockAfter;
        }
        newCost = Math.Round(newCost, 2);

        // 5. Update Ingredient.Cost
        ingredient.Cost = newCost;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[CaculateCostIngredientInStock] '{Name}': Cost {OldCost} → {NewCost} | Stock: {Before} → {After}",
            ingredient.Name, oldCost, newCost, totalStockBefore, totalStockAfter);

        return new CaculateCostIngredientInStockResult
        {
            IngredientId = ingredient.Id,
            IngredientName = ingredient.Name,
            OldCost = oldCost,
            NewCost = newCost,
            TotalStockBefore = totalStockBefore,
            TotalStockAfter = totalStockAfter,
            Message = $"MAC updated: {oldCost} → {newCost} for '{ingredient.Name}' (Stock: {totalStockBefore} → {totalStockAfter})"
        };
    }
}
