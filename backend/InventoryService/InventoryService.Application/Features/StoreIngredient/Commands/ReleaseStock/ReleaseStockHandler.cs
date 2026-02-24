using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Persistences.Repositories;
using MediatR;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ReleaseStock;


/// <summary>
/// Handler xử lý giải phóng hàng đã giữ (Release Stock).
/// Xóa Queue giữ chỗ mà không trừ kho thật (vì chưa thanh toán → kho chưa bị trừ).
/// </summary>
public sealed class ReleaseStockHandler
    : IRequestHandler<ReleaseStockCommand, ReleaseStockRes>
{
    private readonly IStoreIngredientRepository _storeIngredientRepo;

    public ReleaseStockHandler(IStoreIngredientRepository storeIngredientRepo)
    {
        _storeIngredientRepo = storeIngredientRepo;
    }

    public async Task<ReleaseStockRes> Handle(
        ReleaseStockCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // 1. Kiểm tra queue có tồn tại không
        var queueItems = await _storeIngredientRepo
            .GetQueueByOrderIdAsync(request.OrderId, cancellationToken);

        if (queueItems == null || !queueItems.Any())
        {
            return new ReleaseStockRes
            {
                Success = false,
                Message = "No reserved stock found in queue for this order."
            };
        }

        // 2. Xóa queue (giải phóng giữ chỗ)
        // không cần IncreaseStock vì kho thật chưa bị trừ ở bước Reserve.
        // Reserve chỉ ghi vào queue, kho thật chỉ bị trừ khi ConfirmStockReservation.
        await _storeIngredientRepo.RemoveQueueByOrderIdAsync(request.OrderId);
        return new ReleaseStockRes
        {
            Success = true,
            Message = "Stock released successfully and queue cleared."
        };
    }
}
