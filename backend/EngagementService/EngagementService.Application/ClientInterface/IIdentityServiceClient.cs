using EngagementService.Application.Dtos.Indentity;

namespace EngagementService.Application.ClientInterface
{
    public interface IIdentityServiceClient
    {
        Task<UserIdsResp> GetUserIdByEmail(GetUserIdByEmailRequest request);
        Task<UserIdsResp> GetUserIdByPhone(GetUserIdByPhoneRequest request);
    }
}
