using Grpc.Net.Client;
using IdentityService.Application.Dtos.Order.Response;
using IdentityService.Application.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.API;

namespace IdentityService.Infrastructure.ExternalServices
{
    public class SaleServiceClient : ISaleServiceClient
    {
        private readonly OrderGrpc.OrderGrpcClient _client;
        private readonly ILogger<IEngagementServiceClient> _logger;
        public SaleServiceClient(IConfiguration configuration, ILogger<IEngagementServiceClient> logger)
        {
            var identityServiceUrl = configuration["SALES_SERVICE_URL"] ?? "http://localhost:5003";
            var channel = GrpcChannel.ForAddress(identityServiceUrl);
            _client = new OrderGrpc.OrderGrpcClient(channel);
            _logger = logger;
        }
        public async Task<IEnumerable<UserCountOrderRes>> GetCountOrderByUserIds(List<Guid> UserIds)
        {
            if (UserIds == null || !UserIds.Any())
                return Enumerable.Empty<UserCountOrderRes>();

            try
            {
                var request = new GetCountOrderByUserIdsRequest();
                request.UserId.AddRange(UserIds.Select(x => x.ToString()));

                var response = await _client.GetCountOrderByUserIdsAsync(request);

                return response.UserCountOrder.Select(x => new UserCountOrderRes
                {
                    UserId = Guid.Parse(x.UserId),
                    OrderCount = x.OrderCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error calling SalesService GetCountOrderByUserIds");

                throw;
            }
        }
    }
}
