using MediatR;
using Ardalis.Result;
using System.Text.Json.Serialization;

namespace SupplierService.Application.Features.SupplierPayments.Commands.VnPayIpn;

public sealed record VnPayIpnCommand(IDictionary<string, string> QueryParams)
    : IRequest<Result<VnPayIpnResponse>>;

public sealed class VnPayIpnResponse
{
    [JsonPropertyName("RspCode")]
    public string RspCode { get; set; } = string.Empty;
    
    [JsonPropertyName("Message")]
    public string Message { get; set; } = string.Empty;
}
