using InventoryService.Application.AppExceptions;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Features.StoreIngredient.Commands.CheckEnoughStock;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Services;
using MediatR;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ReserveStock;

/// <summary>
/// Handler xử lý yêu cầu giữ hàng (Reserve Stock) cho đơn hàng mới.
/// Logic: Kiểm tra tồn kho khả dụng (Thực tế - Đang giữ) >= Yêu cầu -> Thêm vào hàng đợi (Queue).
/// </summary>
public class ReserveStockHandler : IRequestHandler<ReserveStockCommand, CheckEnoughStockRes>
{
    private readonly IStoreIngredientRepository _storeIngredientRepo;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IInventoryDomainService _domainService;
    private readonly IMediator _mediator;

    public ReserveStockHandler(
        IStoreIngredientRepository storeIngredientRepo,
        IRecipeRepository recipeRepository,
        IInventoryDomainService domainService,
        IMediator mediator)
    {
        _storeIngredientRepo = storeIngredientRepo;
        _recipeRepository = recipeRepository;
        _domainService = domainService;
        _mediator = mediator;
    }

    /// <summary>
    /// Xử lý lệnh giữ hàng.
    /// </summary>
    /// <param name="request">Thông tin yêu cầu giữ hàng (Items, StoreId, OrderId).</param>
    /// <param name="cancellationToken">Token hủy.</param>
    /// <returns>Kết quả giữ hàng (Thành công/Thất bại).</returns>
    public async Task<CheckEnoughStockRes> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any()) //Nếu không có item nào cần check, trả về đủ hàng luôn (Không cần ghi Queue)
        {
            return new CheckEnoughStockRes { IsEnough = true, ShortageItems = new List<AmountVariant>() };
        }

        // Gọi CheckEnoughStock qua MediatR 
        var checkResult = await _mediator.Send(new CheckEnoughStockCommand(new CheckEnoughStockReq
        {
            StoreId = request.StoreId,
            CheckItem = request.Items
        }), cancellationToken);


        //Nếu không đủ hàng,trả kết quả ngay, không ghi Queue
        if (!checkResult.IsEnough)
        {
            return checkResult; //return luôn không ghi vào Queue
        }

        // Đủ hàng, tính toán nguyên liệu cần giữ và chèn vào Queue
        var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();
        var allRecipes = await _recipeRepository.GetRecipesByVariantIdsAsync(variantIds, cancellationToken);

        if (allRecipes == null || !allRecipes.Any())
        {
            throw new BusinessException(Message.Recipe.NotFound);
        }

        var totalRequiredIngredients = _domainService.CalculateRequiredIngredients(
            request.Items.Select(x => (x.VariantId, x.Amount)),
            allRecipes);

        // tạo queue items
        // TỐI ƯU HÓA: Batch Insert (Đảm bảo Atomicity)
        // Thay vì gọi AddToQueueAsync trong vòng lặp (gây ra nhiều lệnh Insert riêng lẻ),
        // ta gom tất cả vào một List và Insert 1 lần duy nhất.
        var queueItems = totalRequiredIngredients.Select(req => new IngredientQueue
        {
            Id = Guid.NewGuid(),
            StoreId = request.StoreId,
            IngredientId = req.Key,
            OrderId = request.OrderId,
            Amount = req.Value,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(16),
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        }).ToList();


        await _storeIngredientRepo.AddToQueueBatchAsync(queueItems);
        return checkResult;
    }
}
