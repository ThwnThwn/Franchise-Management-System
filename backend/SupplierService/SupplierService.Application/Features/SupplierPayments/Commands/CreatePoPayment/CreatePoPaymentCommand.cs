using Ardalis.Result;
using MediatR;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Features.SupplierPayments.Commands.CreatePoPayment;

public sealed record CreatePoPaymentCommand(
    Guid PurchaseOrderId,
    SupplierPaymentMethod Method,
    string? ReturnUrl
) : IRequest<Result<CreatePoPaymentResult>>;


public sealed class CreatePoPaymentResult
{
    public Guid PaymentId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}
