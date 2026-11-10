using MediatR;
using SupplierService.Application.ClientInterfaces;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Features.PurchaseOrders.Commands.CompletePurchaseOrder;

/// <summary>
/// Handler xử lý hoàn thành đơn nhập hàng:
/// 1. Lấy PO từ DB (kèm Items)
/// 2. Validate trạng thái hợp lệ
/// 3. Update Status = Completed, CompletedAt = Now
/// 4. SaveChanges
/// 5. Gọi gRPC IncreaseStock sang InventoryService để cộng kho
/// </summary>
public class CompletePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepo,
    IInventoryServiceClient inventoryClient
) : IRequestHandler<CompletePurchaseOrderCommand>
{
    public async Task<Unit> Handle(CompletePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy PO kèm Items
        var po = await purchaseOrderRepo.GetByIdWithItemsAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order {request.PurchaseOrderId} not found.");

        // 2. Validate — chỉ cho Complete khi đang Pending hoặc Processing
        if (po.Status == PurchaseOrderStatus.Completed)
            throw new InvalidOperationException("Purchase order is already completed.");

        if (po.Status == PurchaseOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled purchase order.");

        if (po.StoreId == null)
            throw new InvalidOperationException("Purchase order has no target store. Please set StoreId before completing.");

        if (!po.Items.Any())
            throw new InvalidOperationException("Purchase order has no items. Cannot complete an empty order.");

        // 3. Update trạng thái
        po.Status = PurchaseOrderStatus.Completed;
        po.PaymentStatus = PaymentStatus.Paid;
        po.CompletedAt = DateTimeOffset.UtcNow;

        // 4. Lưu DB
        await purchaseOrderRepo.SaveChangesAsync(cancellationToken);

        // 5. Gọi gRPC sang InventoryService để cộng kho
        var stockEntries = po.Items.Select(item =>
            new IngredientStockEntry(item.IngredientId, item.Quantity, item.UnitPrice));

        await inventoryClient.IncreaseStockAsync(po.StoreId.Value, stockEntries, cancellationToken);

        return Unit.Value;
    }
}