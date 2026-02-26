using MediatR;

namespace SupplierService.Application.Features.PurchaseOrders.Commands.CompletePurchaseOrder;

/// <summary>
/// Command hoàn thành đơn nhập hàng.
/// Sau khi xử lý: PO.Status = Completed, gọi gRPC IncreaseStock sang InventoryService.
/// </summary>
public record CompletePurchaseOrderCommand(Guid PurchaseOrderId) : IRequest;
