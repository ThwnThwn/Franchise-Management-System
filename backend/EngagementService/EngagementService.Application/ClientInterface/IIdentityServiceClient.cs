using EngagementService.Application.Dtos.Indentity;

namespace EngagementService.Application.ClientInterface
{
    public interface IIdentityServiceClient
    {
        Task<UserIdRep> GetUserIdByEmail(GetUserIdByEmailRequest request);
        Task<UserIdRep> GetUserIdByPhone(GetUserIdByPhoneRequest request);
    }
}
