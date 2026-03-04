
using EngagementService.Application.ClientInterface;
using EngagementService.Application.Dtos.Indentity;
using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User;
using MediatR;

namespace EngagementService.API.Services.Identity
{
    public class IdentityServiceClient : IIdentityServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly string _identityServiceUrl;
        private readonly UserGrpc.UserGrpcClient _client;
        public IdentityServiceClient(IMediator mediator, IConfiguration configuration)
        {
            _configuration = configuration;

            var rawUrl = configuration["IDENTITY_SERVICE_URL"] ?? "http://localhost:5001";
            // Allow env var values like "localhost:5002" by normalizing to an absolute URI.
            _identityServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                 rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? rawUrl
                : $"http://{rawUrl}";

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

        public async Task<UserSimpleResp> GetUserIdByEmail(EngagementService.Application.Dtos.Indentity.GetUserIdByEmailRequest request)
        {
            var response = await _client.GetUserIdByEmailAsync(new IdentityService.Grpc.Protos.User.GetUserIdByEmailRequest { Email = request.Email });
            return new UserSimpleResp { UserIDNameResp = response.UserRes.Select(u => new UserIDNameResp { UserId = Guid.Parse(u.Id), UserName = u.Name }).ToList() };
        }

        public async Task<UserSimpleResp> GetUserIdByPhone(EngagementService.Application.Dtos.Indentity.GetUserIdByPhoneRequest request)
        {
            var response = await _client.GetUserIdByPhoneAsync(new IdentityService.Grpc.Protos.User.GetUserIdByPhoneRequest { Phone = request.PhoneNumber });
            return new UserSimpleResp { UserIDNameResp = response.UserRes.Select(u => new UserIDNameResp { UserId = Guid.Parse(u.Id), UserName = u.Name }).ToList() };
        }
    }
}
