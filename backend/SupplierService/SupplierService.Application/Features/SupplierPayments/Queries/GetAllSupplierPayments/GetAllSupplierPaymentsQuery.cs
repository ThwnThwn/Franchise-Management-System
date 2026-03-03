using MediatR;

namespace SupplierService.Application.Features.SupplierPayments.Queries.GetAllSupplierPayments;

public sealed record GetAllSupplierPaymentsQuery(
    int Page,
    int PageSize,
    string? SupplierId,
    string? PurchaseOrderId
) : IRequest<GetAllSupplierPaymentsResult>;

public sealed class GetAllSupplierPaymentsResult
{
    public IEnumerable<SupplierPaymentSummaryDto> Payments { get; init; } = [];
    public int TotalCount { get; init; }
}

public sealed class SupplierPaymentSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string PurchaseOrderId { get; init; } = string.Empty;
    public string PurchaseOrderCode { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public double Amount { get; init; }
    public int Method { get; init; }
    public int Status { get; init; }
    public string? PaidAt { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
}
