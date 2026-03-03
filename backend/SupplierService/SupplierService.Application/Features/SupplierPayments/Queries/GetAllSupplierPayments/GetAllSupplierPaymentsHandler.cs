using MediatR;
using SupplierService.Application.Repositories;

namespace SupplierService.Application.Features.SupplierPayments.Queries.GetAllSupplierPayments;

public sealed class GetAllSupplierPaymentsHandler
    : IRequestHandler<GetAllSupplierPaymentsQuery, GetAllSupplierPaymentsResult>
{
    private readonly ISupplierPaymentRepository _paymentRepository;

    public GetAllSupplierPaymentsHandler(ISupplierPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<GetAllSupplierPaymentsResult> Handle(
        GetAllSupplierPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        Guid? supplierId = Guid.TryParse(request.SupplierId, out var sid) ? sid : null;
        Guid? purchaseOrderId = Guid.TryParse(request.PurchaseOrderId, out var pid) ? pid : null;

        var (items, totalCount) = await _paymentRepository.GetAllAsync(
            page, pageSize, supplierId, purchaseOrderId, cancellationToken);

        return new GetAllSupplierPaymentsResult
        {
            TotalCount = totalCount,
            Payments = items.Select(p => new SupplierPaymentSummaryDto
            {
                Id = p.Id.ToString(),
                PurchaseOrderId = p.PurchaseOrderId.ToString(),
                PurchaseOrderCode = p.PurchaseOrder?.Code ?? string.Empty,
                Code = p.Code,
                Amount = (double)p.Amount,
                Method = p.Method,
                Status = p.Status,
                PaidAt = p.PaidAt?.ToString("O"),
                CreatedAt = p.CreatedAt.ToString("O")
            })
        };
    }
}
