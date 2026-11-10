using MediatR;
using SupplierService.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

namespace SupplierService.Application.Features.PurchaseOrders.Commands.UpdatePurchaseOrder;

/// <summary>
/// Command cập nhật đơn nhập hàng (chỉ khi Status = Pending).
/// </summary>
public record UpdatePurchaseOrderCommand(
    Guid Id,
    Guid SupplierId,
    Guid? StoreId,
    string? Notes,
    string? ReferenceCode,
    DateTimeOffset? ExpectedDeliveryDate,
    decimal DiscountAmount,
    List<CreatePurchaseOrderItemDto> Items
) : IRequest;
