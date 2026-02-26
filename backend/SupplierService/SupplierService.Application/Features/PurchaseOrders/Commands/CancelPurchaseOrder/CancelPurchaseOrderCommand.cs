using MediatR;

namespace SupplierService.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;

/// <summary>
/// Command hủy đơn nhập hàng (chỉ khi Pending hoặc Processing).
/// </summary>
public record CancelPurchaseOrderCommand(Guid Id, string? Reason) : IRequest;
