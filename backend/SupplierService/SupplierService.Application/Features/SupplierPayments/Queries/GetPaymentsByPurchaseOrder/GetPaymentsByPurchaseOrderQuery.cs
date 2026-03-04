using MediatR;

namespace SupplierService.Application.Features.SupplierPayments.Queries.GetPaymentsByPurchaseOrder;

public sealed record GetPaymentsByPurchaseOrderQuery(Guid PurchaseOrderId)
    : IRequest<GetPaymentsByPurchaseOrderResult>;

public sealed class GetPaymentsByPurchaseOrderResult
{
    public IEnumerable<SupplierPaymentDto> Payments { get; init; } = [];
}

public sealed class SupplierPaymentDto
{
    public string Id { get; init; } = string.Empty;
    public string PurchaseOrderId { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public double Amount { get; init; }
    public int Method { get; init; }
    public int Status { get; init; }
    public string? Notes { get; init; }
    public string? PaidAt { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
}
