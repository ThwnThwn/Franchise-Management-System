using Ardalis.Result;
using MediatR;

namespace SupplierService.Application.Features.SupplierPayments.Commands.VnPayReturn;

public sealed record VnPayReturnCommand(IDictionary<string, string> QueryParams)
    : IRequest<Result<VnPayReturnResult>>;

public sealed class VnPayReturnResult
{
    public bool Success { get; set; }
    public Guid PaymentId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string ResponseCode { get; set; } = default!;
}
