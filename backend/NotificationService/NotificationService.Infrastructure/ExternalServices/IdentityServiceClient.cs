using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Application.ExternalServices;

namespace NotificationService.Infrastructure.ExternalServices;

/// <summary>
/// Triển khai Client gọi gRPC sang Identity Service.
/// </summary>
public class IdentityServiceClient : IIdentityServiceClient
{
    private readonly UserGrpc.UserGrpcClient _client;
    private readonly ILogger<IdentityServiceClient> _logger;

    public IdentityServiceClient(IConfiguration configuration, ILogger<IdentityServiceClient> logger)
    {
        var identityServiceUrl = configuration["IDENTITY_SERVICE_URL"] ?? "http://localhost:5001";
        var channel = GrpcChannel.ForAddress(identityServiceUrl);
        _client = new UserGrpc.UserGrpcClient(channel);
        _logger = logger;
    }

    public async Task<string> GetUserEmailByIdAsync(string userId)
    {
        try
        {
            var request = new GetUserByIdRequest { Id = userId };
            var response = await _client.GetByIdAsync(request);
            return response.Email;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get email for user {UserId} from IdentityService", userId);
            return string.Empty;
        }
    }
}
