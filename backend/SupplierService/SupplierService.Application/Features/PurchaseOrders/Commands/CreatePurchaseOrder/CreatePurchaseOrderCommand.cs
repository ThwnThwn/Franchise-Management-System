using MediatR;

namespace SupplierService.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

/// <summary>
/// Command tạo mới đơn nhập hàng.
/// </summary>
public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    Guid? StoreId,
    string? Notes,
    string? ReferenceCode,
    DateTimeOffset? ExpectedDeliveryDate,
    decimal DiscountAmount,
    List<CreatePurchaseOrderItemDto> Items
) : IRequest<CreatePurchaseOrderResult>;

public record CreatePurchaseOrderItemDto(
    Guid IngredientId,
    decimal Quantity
);

public record CreatePurchaseOrderResult(Guid Id, string Code);
