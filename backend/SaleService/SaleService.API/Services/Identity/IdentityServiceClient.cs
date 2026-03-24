using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User;
using SalesService.Application.Interfaces.Identity;

namespace SalesService.API.Services.Identity;

public class IdentityServiceClient(UserGrpc.UserGrpcClient client) : IIdentityServiceClient
{
    public async Task<UserInfo?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var request = new GetUserByIdRequest { Id = userId.ToString() };
            var response = await client.GetByIdAsync(request);

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
            Console.WriteLine($"Error calling IdentityService: {ex.Message}");
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

            var response = await client.GetByIdsAsync(request);

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
            Console.WriteLine($"Error calling IdentityService GetUsersByIds: {ex.Message}");
            return new List<UserInfo>();
        }
    }
}