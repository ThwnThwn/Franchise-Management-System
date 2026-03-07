using MediatR;
using Ardalis.Result;

namespace SalesService.Application.Features.Payments.VnPayIpn;

public sealed record VnPayIpnCommand(IDictionary<string, string> QueryParams)
    : IRequest<Result<VnPayIpnResponse>>;

public sealed class VnPayIpnResponse
{
    public string RspCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
