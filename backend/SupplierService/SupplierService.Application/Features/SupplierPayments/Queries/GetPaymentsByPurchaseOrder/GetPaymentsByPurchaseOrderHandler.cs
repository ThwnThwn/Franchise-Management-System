using MediatR;
using SupplierService.Application.Repositories;

namespace SupplierService.Application.Features.SupplierPayments.Queries.GetPaymentsByPurchaseOrder;

public sealed class GetPaymentsByPurchaseOrderHandler
    : IRequestHandler<GetPaymentsByPurchaseOrderQuery, GetPaymentsByPurchaseOrderResult>
{
    private readonly ISupplierPaymentRepository _paymentRepository;

    public GetPaymentsByPurchaseOrderHandler(ISupplierPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<GetPaymentsByPurchaseOrderResult> Handle(
        GetPaymentsByPurchaseOrderQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByPurchaseOrderIdAsync(request.PurchaseOrderId, cancellationToken);

        return new GetPaymentsByPurchaseOrderResult
        {
            Payments = payments.Select(p => new SupplierPaymentDto
            {
                Id = p.Id.ToString(),
                PurchaseOrderId = p.PurchaseOrderId.ToString(),
                Code = p.Code,
                Amount = (double)p.Amount,
                Method = p.Method,
                Status = p.Status,
                Notes = p.Notes,
                PaidAt = p.PaidAt?.ToString("O"),
                CreatedAt = p.CreatedAt.ToString("O")
            })
        };
    }
}
