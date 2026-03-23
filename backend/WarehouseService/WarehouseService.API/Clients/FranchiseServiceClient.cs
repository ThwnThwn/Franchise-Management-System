using Grpc.Net.Client;
using WarehouseService.Application.ClientInterfaces;
using FranchiseService.Grpc;

namespace WarehouseService.API.Clients
{
    public class FranchiseServiceClient : IFranchiseServiceClient
    {
        private readonly StoreGrpcService.StoreGrpcServiceClient _grpcClient;
        public FranchiseServiceClient(IConfiguration configuration)
        {
            var rawUrl = configuration["FRANCHISE_SERVICE_URL"] ?? "http://localhost:5006";
            var storeServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                      rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? rawUrl
                : $"http://{rawUrl}";

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

        public async Task<bool> StoreExistsAsync(Guid storeId)
        {
            try
            {
                var request = new GetStoreDetailRequest { Id = storeId.ToString() };
                var response = await _grpcClient.GetStoreDetailAsync(request);

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
