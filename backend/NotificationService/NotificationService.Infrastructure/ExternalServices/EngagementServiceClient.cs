using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EngagementService.Infrastructure.Protos; 
using NotificationService.Application.ExternalServices;

namespace NotificationService.Infrastructure.ExternalServices;

public class EngagementServiceClient : IEngagementServiceClient
{
    private readonly LoyaltyAccountGrpc.LoyaltyAccountGrpcClient _client;
    private readonly ILogger<EngagementServiceClient> _logger;

    public EngagementServiceClient(
        IConfiguration configuration,
        ILogger<EngagementServiceClient> logger)
    {
        var serviceUrl = configuration["ENGAGEMENT_SERVICE_URL"]?? "https://localhost:5007";

        var channel = GrpcChannel.ForAddress(serviceUrl);
        _client = new LoyaltyAccountGrpc.LoyaltyAccountGrpcClient(channel);
        _logger = logger;
    }

    public async Task<string> CreateLoyaltyAccount(Guid userId)
    {
        try
        {

            var response = await _client.CreateLoyaltyAccountAsync(new CreateLoyaltyAccountRequest
            {
                UserId = userId.ToString()
            });

            return "Create loyalty for userID " + userId; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create loyalty account for user {UserId}",
                userId);

            return string.Empty;
        }
    }
}