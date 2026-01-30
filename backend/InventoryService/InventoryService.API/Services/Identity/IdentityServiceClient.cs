using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User.Internal;
using InventoryService.Application.ClientInterface.Indentity;
using InventoryService.Application.Dtos.User;
using MediatR;

namespace InventoryService.API.Services.Identity
{
    public class IdentityServiceClient : IIdentityServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly string _identityServiceUrl;
        private readonly UserGrpcInternal.UserGrpcInternalClient _client;
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

            _client = new UserGrpcInternal.UserGrpcInternalClient(channel);
        }
        public Task<UserRep> GetManager(GetManagerReq request)
        {
            throw new NotImplementedException();
        }
    }
}
