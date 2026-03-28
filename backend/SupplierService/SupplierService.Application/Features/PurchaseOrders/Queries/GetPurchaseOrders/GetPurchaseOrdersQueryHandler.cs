using MediatR;
using Microsoft.EntityFrameworkCore;
using SupplierService.Application.Repositories;

namespace SupplierService.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;

public class GetPurchaseOrdersQueryHandler(IPurchaseOrderRepository repository
) : IRequestHandler<GetPurchaseOrdersQuery, GetPurchaseOrdersResult>
{
    public async Task<GetPurchaseOrdersResult> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable()
            .Include(po => po.Supplier)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.SupplierId) && Guid.TryParse(request.SupplierId, out var supplierId))
        {
            query = query.Where(po => po.SupplierId == supplierId);
        }

        if (!string.IsNullOrEmpty(request.StoreId) && Guid.TryParse(request.StoreId, out var storeId))
        {
            query = query.Where(po => po.StoreId == storeId);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(po => (int)po.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        var orders = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = new GetPurchaseOrdersResult
        {
            TotalCount = totalCount,
            PurchaseOrders = orders.Select(po => new PurchaseOrderSummaryDto
            {
                Id = po.Id.ToString(),
                Code = po.Code,
                SupplierName = po.Supplier?.Name ?? "",
                Status = (int)po.Status,
                PaymentStatus = (int)po.PaymentStatus,
                TotalAmount = (double)po.TotalAmount,
                NetAmount = (double)po.NetAmount,
                OrderDate = po.OrderDate.ToString("O"),
                CreatedAt = po.CreatedAt.ToString("O")
            })
        };

        return result;
    }
}