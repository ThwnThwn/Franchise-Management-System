using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User;
using SalesService.Application.Interfaces.Identity;

namespace SalesService.API.Services.Identity;

public class IdentityServiceClient : IIdentityServiceClient
{
    private readonly UserGrpc.UserGrpcClient _UserClient;
    private readonly string _identityServiceUrl;
    public IdentityServiceClient(IConfiguration configuration)
    {
        var rawUrl = configuration["IDENTITY_SERVICE_URL"] ?? "http://localhost:5001";

        // Normalize to absolute URI
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

        _UserClient = new UserGrpc.UserGrpcClient(channel);
    }

    public async Task<UserInfo?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var request = new GetUserByIdRequest { Id = userId.ToString() };
            var response = await _UserClient.GetByIdAsync(request);

            if (response == null)
                return null;

            return new UserInfo
            {
                Id = Guid.Parse(response.Id),
                Email = response.Email,
                Username = response.Username,
                Phone = response.Phone,
                Role = response.Role != null ? new UserRole
                {
                    Id = Guid.Parse(response.Role.Id),
                    RoleName = response.Role.Name,
                    Slug=response.Role.Slug
                } : null,
                LoyaltyTier = response.Loyalty != null ? new LoyaltyRes
                {
                    Id = Guid.Parse(response.Loyalty.Id),
                    AvailablePoints = decimal.Parse(response.Loyalty.AvailablePoints),
                    CurrentTier = response.Loyalty.CurrentTier == null ? null : new Application.Interfaces.Identity.MembershipTier
                    {
                        Id = Guid.Parse(response.Loyalty.CurrentTier.Id),
                        Name = response.Loyalty.CurrentTier.Name,
                        MinSpendRequired = decimal.Parse(response.Loyalty.CurrentTier.MinSpendRequired),
                        DiscountRate = response.Loyalty.CurrentTier.DiscountRate,
                        AmountPerPoint = decimal.Parse(response.Loyalty.CurrentTier.AmountPerPoint),
                        ValuePerPoint = decimal.Parse(response.Loyalty.CurrentTier.ValuePerPoint)
                    }
                } : null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling IdentityService ({_identityServiceUrl}): {ex.Message}");
            return null; // Return null instead of throwing - order can continue without email
        }
    }

    public async Task<List<UserInfo>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        if (userIds == null || !userIds.Any())
            return new List<UserInfo>();

        try
        {
            var request = new GetUserByIdsRequest();
            request.Ids.AddRange(userIds.Select(id => id.ToString()));

            var response = await _UserClient.GetByIdsAsync(request);

            return response.Users.Select(u => new UserInfo
            {
                Id = Guid.TryParse(u.Id, out var id) ? id : Guid.Empty,  
                Email = u.Email ?? "",
                Username = u.Username ?? "",
                Phone = string.IsNullOrEmpty(u.Phone) ? null : u.Phone,
                Role = u.Role != null && !string.IsNullOrEmpty(u.Role.Id) ? new UserRole
                {
                    Id = Guid.TryParse(u.Role.Id, out var roleId) ? roleId : Guid.Empty,  
                    RoleName = u.Role.Name ?? "",
                    Slug = u.Role.Slug ?? ""
                } : null
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling IdentityService GetUsersByIds ({_identityServiceUrl}): {ex.Message}");
            return new List<UserInfo>();
        }
    }
}