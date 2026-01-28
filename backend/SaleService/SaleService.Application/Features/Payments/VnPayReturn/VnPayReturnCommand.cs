using Ardalis.Result;
using MediatR;

namespace SalesService.Application.Features.Payments.VnPayReturn;

public sealed record VnPayReturnCommand(IDictionary<string, string> QueryParams)
    : IRequest<Result<VnPayReturnResponse>>;

public sealed class VnPayReturnResponse
{
    public bool Success { get; set; }
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public string ResponseCode { get; set; } = default!;
}

