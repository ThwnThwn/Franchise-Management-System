using MediatR;

namespace SupplierService.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;

public record GetPurchaseOrdersQuery(
    string? SupplierId,
    string? StoreId,
    int? Status,
    int Page,
    int PageSize
) : IRequest<GetPurchaseOrdersResult>;
