using MediatR;
using SupplierService.Application.Repositories;

namespace SupplierService.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdQueryHandler(IPurchaseOrderRepository repository
) : IRequestHandler<GetPurchaseOrderByIdQuery, GetPurchaseOrderByIdResult>
{
    public async Task<GetPurchaseOrderByIdResult> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var poId))
        {
            throw new Exception("Invalid PurchaseOrder Id");
        }

        var po = await repository.GetByIdWithItemsAsync(poId, cancellationToken);

        if (po == null)
        {
            throw new Exception($"Purchase order {poId} not found");
        }

        var detail = new GetPurchaseOrderByIdResult
        {
            Id = po.Id.ToString(),
            Code = po.Code,
            SupplierId = po.SupplierId.ToString(),
            SupplierName = po.Supplier?.Name ?? "",
            StoreId = po.StoreId?.ToString() ?? "",
            Status = (int)po.Status,
            PaymentStatus = (int)po.PaymentStatus,
            TotalAmount = (double)po.TotalAmount,
            DiscountAmount = (double)po.DiscountAmount,
            NetAmount = (double)po.NetAmount,
            PaidAmount = (double)po.PaidAmount,
            OrderDate = po.OrderDate.ToString("O"),
            CompletedAt = po.CompletedAt?.ToString("O") ?? "",
            Notes = po.Notes ?? "",
            CreatedAt = po.CreatedAt.ToString("O"),
            UpdatedAt = po.UpdatedAt.ToString("O"),
            Items = po.Items.Select(item => new PurchaseOrderItemDetailDto
            {
                Id = item.Id.ToString(),
                IngredientId = item.IngredientId.ToString(),
                Quantity = (double)item.Quantity,
                UnitPrice = (double)item.UnitPrice,
                TotalPrice = (double)item.TotalPrice
            })
        };

        return detail;
    }
}