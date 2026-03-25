using Grpc.Net.Client;
using WarehouseService.Application.ClientInterfaces;
using FranchiseService.Grpc;

namespace WarehouseService.API.Clients
{
    public class FranchiseServiceClient(StoreGrpcService.StoreGrpcServiceClient client) : IFranchiseServiceClient
    {
        public async Task<bool> StoreExistsAsync(Guid storeId)
        {
            try
            {
                var request = new GetStoreDetailRequest { Id = storeId.ToString() };
                var response = await client.GetStoreDetailAsync(request);

                return response != null && response.Id == storeId.ToString();
            }
            catch (global::Grpc.Core.RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.NotFound)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
