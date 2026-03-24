using Grpc.Net.Client;
using SupplierService.Grpc.Protos.Supplier;
using WarehouseService.Application.ClientInterfaces;

namespace WarehouseService.API.Clients;

public class SupplierServiceClient : ISupplierServiceClient
{
    private readonly SupplierGrpc.SupplierGrpcClient _client;

    public SupplierServiceClient(IConfiguration configuration)
    {
        var rawUrl = configuration["SUPPLIER_SERVICE_URL"] ?? "http://localhost:5008";
        var supplierServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                 rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? rawUrl
            : $"http://{rawUrl}";

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var channel = GrpcChannel.ForAddress(supplierServiceUrl, new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        _client = new SupplierGrpc.SupplierGrpcClient(channel);
    }

    public async Task<bool> SupplierExistsAsync(Guid supplierId)
    {
        try
        {
            var request = new GetSupplierByIdRequest { Id = supplierId.ToString() };
            var response = await _client.GetSupplierByIdAsync(request);
            return response != null && response.Id == supplierId.ToString();
        }
        catch (global::Grpc.Core.RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.NotFound)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}