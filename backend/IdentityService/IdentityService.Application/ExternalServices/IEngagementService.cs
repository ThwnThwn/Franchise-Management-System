using IdentityService.Application.Dtos.Loyalty.Response;

namespace IdentityService.Application.ExternalServices
{
    public interface IEngagementServiceClient
    {
        Task<LoyaltyRes> GetLoyaltyByEmail(string email);
    }
}
