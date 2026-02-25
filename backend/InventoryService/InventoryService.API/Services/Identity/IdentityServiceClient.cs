using Grpc.Core;
using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User;
using InventoryService.API.Mapper;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.User;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace InventoryService.API.Services.Identity
{
    public class IdentityServiceClient : IIdentityServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly string _identityServiceUrl;
        private readonly UserGrpc.UserGrpcClient _client;
        public IdentityServiceClient(IMediator mediator, IConfiguration configuration)
        {
            _configuration = configuration;

            var rawUrl = configuration["IDENTITY_SERVICE_URL"] ?? "https://localhost:5001";
            // Allow env var values like "localhost:5002" by normalizing to an absolute URI.
            _identityServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                 rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? rawUrl
                : $"https://{rawUrl}";

            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress(_identityServiceUrl, new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

            _client = new UserGrpc.UserGrpcClient(channel);
        }
        public async Task<UserRep> GetManager(GetManagerReq command)
        {
            var request = new GetManagerRequest
            {
                Id = command.StoreId.ToString()
            };
            var response = await _client.GetManagerAsync(request);

            if (response == null) throw new RpcException(new Status(StatusCode.NotFound, Message.User.NotFound));

            return response.ToEntity();

        }
    }
}
