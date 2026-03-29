using MediatR;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;

/// <summary>
/// Handler hủy đơn nhập hàng:
/// 1. Load PO
/// 2. Validate: chỉ cho hủy khi Pending hoặc Processing
/// 3. Set Status = Cancelled, ghi reason vào Notes
/// 4. Save
/// </summary>
public class CancelPurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepo
) : IRequestHandler<CancelPurchaseOrderCommand>
{
    public async Task<Unit> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Load PO
        var po = await purchaseOrderRepo.GetByIdWithItemsAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order {request.Id} not found.");

        // 2. Validate
        if (po.Status == PurchaseOrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed purchase order.");

        if (po.Status == PurchaseOrderStatus.Cancelled)
            throw new InvalidOperationException("Purchase order is already cancelled.");

        // 3. Update
        po.Status = PurchaseOrderStatus.Cancelled;

        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            po.Notes = string.IsNullOrWhiteSpace(po.Notes)
                ? $"[Cancelled] {request.Reason}"
                : $"{po.Notes}\n[Cancelled] {request.Reason}";
        }

        // 4. Save
        await purchaseOrderRepo.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}