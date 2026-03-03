using MediatR;

namespace SupplierService.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

public record GetPurchaseOrderByIdQuery(string Id) : IRequest<GetPurchaseOrderByIdResult>;
