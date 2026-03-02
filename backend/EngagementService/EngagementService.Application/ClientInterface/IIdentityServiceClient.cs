using EngagementService.Application.Dtos.Indentity;

namespace EngagementService.Application.ClientInterface
{
    public interface IIdentityServiceClient
    {
        Task<UserSimpleResp> GetUserIdByEmail(GetUserIdByEmailRequest request);
        Task<UserSimpleResp> GetUserIdByPhone(GetUserIdByPhoneRequest request);
    }
}
