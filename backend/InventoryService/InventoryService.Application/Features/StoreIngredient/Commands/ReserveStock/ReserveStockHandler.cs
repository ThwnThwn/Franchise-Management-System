using InventoryService.Application.Abstractions;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Services;
using MediatR;
using static InventoryService.Application.Constants.Message;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ReserveStock;

/// <summary>
/// Handler xử lý yêu cầu giữ hàng (Reserve Stock) cho đơn hàng mới.
/// Logic: Kiểm tra tồn kho khả dụng (Thực tế - Đang giữ) >= Yêu cầu -> Thêm vào hàng đợi (Queue).
/// </summary>
public class ReserveStockHandler : IRequestHandler<ReserveStockCommand, CheckEnoughStockRes>
{
    private readonly IStoreIngredientRepository _storeIngredientRepo;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IInventoryDbContext _dbContext;
    private readonly IInventoryDomainService _domainService;

    public ReserveStockHandler(
        IStoreIngredientRepository storeIngredientRepo,
        IRecipeRepository recipeRepository,
        IInventoryDbContext dbContext,
        IInventoryDomainService domainService)
    {
        _storeIngredientRepo = storeIngredientRepo;
        _recipeRepository = recipeRepository;
        _dbContext = dbContext;
        _domainService = domainService;
    }

    /// <summary>
    /// Xử lý lệnh giữ hàng.
    /// </summary>
    /// <param name="request">Thông tin yêu cầu giữ hàng (Items, StoreId, OrderId).</param>
    /// <param name="cancellationToken">Token hủy.</param>
    /// <returns>Kết quả giữ hàng (Thành công/Thất bại).</returns>
    public async Task<CheckEnoughStockRes> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
        {
            return new CheckEnoughStockRes { IsEnough = true, ShortageItems = new List<AmountVariant>() };
        }

        //// Lấy tất cả công thức liên quan đến các Variant trong đơn hàng
        //var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();
        //var allRecipes = await _recipeRepository.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);

        //if (allRecipes == null || !allRecipes.Any())
        //{
        //    return new ReserveStockResponseDto { Success = false, Message = Message.Recipe.NotFound };
        //}

        //// Tính toán nguyên liệu dựa trên Domain Service
        //var totalRequiredIngredients = _domainService.CalculateRequiredIngredients(
        //    request.Items.Select(x => (x.VariantId, x.Amount)),
        //    allRecipes);

        //var ingredientIds = totalRequiredIngredients.Keys.ToList();

        //// 2. Tải thông tin Tồn kho Vật lý (Physical Stock) từ DB
        //// Tồn kho vật lý (Physical Stock) là số lượng tồn kho thực tế trong kho
        //var physicalStocks = await _storeIngredientRepo.GetStockByStoreIdAndIngredientIdsAsync(request.StoreId, ingredientIds, cancellationToken);

        //// 3. Kiểm tra & Giữ hàng (Nguyên tắc: Có đủ tất cả hoặc Không gì cả - All or Nothing)
        //// Cần kiểm tra: (Tồn kho thực - Đã giữ trong Queue) >= Yêu cầu
        ////chỗ này có thể race condition, nhưng xác xuất rất thấp -> nếu có khả năng -> thì hoàn tiền/xinloi khách
        //foreach (var req in totalRequiredIngredients)
        //{
        //    var ingredientId = req.Key;
        //    var neededAmount = req.Value;

        //    var physicalItem = physicalStocks.FirstOrDefault(x => x.IngredientId == ingredientId);
        //    var physicalQty = physicalItem?.Quantity ?? 0;

        //    var currentReserved = await _storeIngredientRepo.GetReservedQuantityAsync(request.StoreId, ingredientId);

        //    var availableParams = physicalQty - currentReserved;

        //    if (availableParams < neededAmount)
        //    {
        //        var ingName = physicalItem?.Ingredient?.Name ?? "Unknown";
        //        return new ReserveStockResponseDto
        //        {
        //            Success = false,
        //            Message = $"Not enough stock for {ingName}. Available: {availableParams}, Required: {neededAmount}"
        //        };
        //    }
        //}


        //Trên kia là code cũ của Hưng mặp, giờ đổi thành code mới như bên dưới:
        var response = new CheckEnoughStockRes { IsEnough = true };
        var shortageItems = new List<AmountVariant>();

        var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();
        var allRecipes = await _recipeRepository.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);

        if (allRecipes == null || !allRecipes.Any())
        {
            throw new DirectoryNotFoundException(Message.Recipe.NotFound);
        }
        var totalRequiredIngredients = _domainService.CalculateRequiredIngredients(
            request.Items.Select(x => (x.VariantId, x.Amount)),
            allRecipes);

        // 2. Tải thông tin Tồn kho Vật lý (Physical Stock) từ DB
        // Tồn kho vật lý (Physical Stock) là số lượng tồn kho thực tế trong kho
        var ingredientIds = totalRequiredIngredients.Keys.ToList();
        var physicalStocks = await _storeIngredientRepo.GetStockByStoreIdAndIngredientIdsAsync(request.StoreId, ingredientIds, cancellationToken);

        //tạo directory để dễ thao tác
        var workingStockDict = physicalStocks.ToDictionary(s => s.IngredientId, s => s.Quantity);

        // 4. Kiểm tra từng Item trong yêu cầu
        foreach (var item in request.Items)
        {
            var itemRecipes = allRecipes.Where(r => r.VariantId == item.VariantId).ToList();

            // Giả định ban đầu là có thể pha đủ số lượng yêu cầu
            decimal maxPossible = item.Amount;

            foreach (var recipe in itemRecipes)
            {
                workingStockDict.TryGetValue(recipe.IngredientId, out decimal? currentStock);

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
                shortageItems.Add(new AmountVariant
                {
                    VariantId = item.VariantId,
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
        // 4. Chèn vào Hàng đợi (IngredientQueue)
        try
        {
            // Tạo các item trong hàng đợi
            // TỐI ƯU HÓA: Batch Insert (Đảm bảo Atomicity)
            // Thay vì gọi AddToQueueAsync trong vòng lặp (gây ra nhiều lệnh Insert riêng lẻ),
            // ta gom tất cả vào một List và Insert 1 lần duy nhất.
            var queueItems = new List<IngredientQueue>();

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
                queueItems.Add(queueItem);
            }

            // Gọi Repo 1 lần duy nhất -> 1 Transaction -> An toàn dữ liệu.
            await _storeIngredientRepo.AddToQueueBatchAsync(queueItems);
        }
        catch (Exception ex)
        {
            throw new Exception("Error saving queue");
        }

        return response;
    }
}
