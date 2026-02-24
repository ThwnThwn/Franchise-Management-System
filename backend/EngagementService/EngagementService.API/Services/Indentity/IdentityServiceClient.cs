
using EngagementService.Application.ClientInterface;
using EngagementService.Application.Dtos.Indentity;
using Grpc.Core;
using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User;
using MediatR;

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

        public async Task<UserIdRep> GetUserIdByEmail(EngagementService.Application.Dtos.Indentity.GetUserIdByEmailRequest request)
        {
            var response = await _client.GetUserIdByEmailAsync(new IdentityService.Grpc.Protos.User.GetUserIdByEmailRequest { Email = request.Email });
            return new UserIdRep { UserId = Guid.Parse(response.Id) };
        }

        public async Task<UserIdRep> GetUserIdByPhone(EngagementService.Application.Dtos.Indentity.GetUserIdByPhoneRequest request)
        {
            var response = await _client.GetUserIdByPhoneAsync(new IdentityService.Grpc.Protos.User.GetUserIdByPhoneRequest { Phone = request.PhoneNumber });
            return new UserIdRep { UserId = Guid.Parse(response.Id) };
        }
    }
}
