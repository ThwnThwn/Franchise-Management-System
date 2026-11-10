using FranchiseService.Grpc;
using Grpc.Net.Client;
using SupplierService.Application.ClientInterfaces;

namespace SupplierService.API.Clients.Franchise
{
    public class FranchiseServiceClient : IFranchiseServiceClient
    {
        private readonly StoreGrpcService.StoreGrpcServiceClient _grpcClient;
        public FranchiseServiceClient(StoreGrpcService.StoreGrpcServiceClient client)
        {
            _grpcClient = client;
        }

        public async Task<bool> GetFranchiseByIdAsync(Guid franchiseId)
        {
            try
            {
                var request = new GetStoreDetailRequest { Id = franchiseId.ToString() };
                var response = await _grpcClient.GetStoreDetailAsync(request);
                
                // Trả về true nếu ID khớp (nghĩa là store này có tồn tại)
                return response != null && response.Id == franchiseId.ToString();
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
