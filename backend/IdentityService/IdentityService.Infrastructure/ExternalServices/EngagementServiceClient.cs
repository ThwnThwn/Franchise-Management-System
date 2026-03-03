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
            var identityServiceUrl = configuration["ENGAGEMENT_SERVICE_URL"] ?? "http://localhost:5007";
            var channel = GrpcChannel.ForAddress(identityServiceUrl);
            _client = new LoyaltyAccountGrpc.LoyaltyAccountGrpcClient(channel);
            _logger = logger;
        }

        public async Task<IEnumerable<LoyaltyRes>> GetByFilter( Guid? tierId, decimal? startAt, decimal? endAt) 
        {
            var result = new List<LoyaltyRes>();

            try
            {
                var request = new GetByFilterRequest();

                // Optional filters
                if (tierId.HasValue)
                    request.TierId = tierId.Value.ToString();

                if (startAt.HasValue)
                    request.StartAt = (double)startAt.Value;

                if (endAt.HasValue)
                    request.EndAt = (double)endAt.Value;

                var response = await _client.GetByFilterAsync(request);

                foreach (var loyalty in response.AccountsRes)
                {
                    var res = new LoyaltyRes
                    {
                        Id = Guid.Parse(loyalty.Id),
                        AvailablePoints = decimal.Parse(loyalty.AvailablePoints),
                        TotalSpent = decimal.Parse(loyalty.TotalSpent),
                        CurrentTier = new MembershipTier
                        {
                            Id = Guid.Parse(loyalty.CurrentTier.Id),
                            Name = loyalty.CurrentTier.Name,
                            ValuePerPoint = decimal.Parse(loyalty.CurrentTier.ValuePerPoint),
                            MinSpendRequired = decimal.Parse(loyalty.CurrentTier.MinSpendRequired),
                            AmountPerPoint = decimal.Parse(loyalty.CurrentTier.AmountPerPoint),
                            DiscountRate = loyalty.CurrentTier.DiscountRate
                        }
                    };

                    result.Add(res);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get loyalty accounts for {Count} users",0);

                return result;
            }
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
