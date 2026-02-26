using FranchiseService.Grpc;
using Grpc.Net.Client;
using SupplierService.Application.ClientInterfaces;

namespace SupplierService.API.Clients.Franchise
{
    public class FranchiseServiceClient : IFranchiseServiceClient
    {
        private readonly StoreGrpcService.StoreGrpcServiceClient _grpcClient;
        public FranchiseServiceClient(IConfiguration configuration)
        {
            var rawUrl = configuration["FRANCHISE_SERVICE_URL"] ?? "https://localhost:5006";
            var storeServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                      rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? rawUrl
                : $"https://{rawUrl}";

            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress(storeServiceUrl, new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

            _grpcClient = new StoreGrpcService.StoreGrpcServiceClient(channel);
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
