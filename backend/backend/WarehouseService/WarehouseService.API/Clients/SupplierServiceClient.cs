using Grpc.Net.Client;
using SupplierService.Grpc.Protos.Supplier;
using WarehouseService.Application.ClientInterfaces;

namespace WarehouseService.API.Clients;

public class SupplierServiceClient(SupplierGrpc.SupplierGrpcClient client) : ISupplierServiceClient
{
    public async Task<bool> SupplierExistsAsync(Guid supplierId)
    {
        try
        {
            var request = new GetSupplierByIdRequest { Id = supplierId.ToString() };
            var response = await client.GetSupplierByIdAsync(request);
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