using EngagementService.Grpc.Protos.LoyaltyAccountGrpc;
using EngagementService.Grpc.Protos.MembershipTier;
using Grpc.Net.Client;
using IdentityService.Application.Dtos.Loyalty.Response;
using IdentityService.Application.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.ExternalServices
{
    public class EngagementServiceClient : IEngagementServiceClient
    {
        
        private readonly LoyaltyAccountGrpc.LoyaltyAccountGrpcClient _client;
        private readonly ILogger<IEngagementServiceClient> _logger;
        public EngagementServiceClient(IConfiguration configuration, ILogger<IEngagementServiceClient> logger)
        {
            var identityServiceUrl = configuration["ENGAGEMENT_SERVICE_URL"] ?? "https://localhost:5007";
            var channel = GrpcChannel.ForAddress(identityServiceUrl);
            _client = new LoyaltyAccountGrpc.LoyaltyAccountGrpcClient(channel);
            _logger = logger;
        }

        public async Task<LoyaltyRes> GetLoyaltyByEmail(string email)
        {
            LoyaltyRes res = new LoyaltyRes();
            try
            {   
                var request = new GetByUserPhoneOrEmailRequest
                {
                    Email = email
                };
                var response = await _client.GetByUserPhoneOrEmailAsync(request);
                var loyalty = response.AccountsRes.FirstOrDefault();
                MembershipTier currenttier = new MembershipTier();

                if (loyalty != null)
                { 
                    res.Id = Guid.Parse(loyalty.Id);
                    //match currenttier
                    currenttier.Id = Guid.Parse(loyalty.CurrentTier.Id);
                    currenttier.Name = loyalty.CurrentTier.Name;
                    currenttier.ValuePerPoint = Decimal.Parse(loyalty.CurrentTier.ValuePerPoint);
                    currenttier.MinSpendRequired = Decimal.Parse(loyalty.CurrentTier.MinSpendRequired);
                    currenttier.AmountPerPoint = Decimal.Parse(loyalty.CurrentTier.AmountPerPoint);
                    currenttier.DiscountRate = loyalty.CurrentTier.DiscountRate;

                    res.CurrentTier = currenttier;
                    res.AvailablePoints = Decimal.Parse(loyalty.AvailablePoints);
                    res.TotalSpent = Decimal.Parse(loyalty.TotalSpent);
                }
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get loyaltt for user {UserId} from Engagement by email", email);
                return res;
            }
        }
    }
    
}
