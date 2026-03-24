
using EngagementService.Application.ClientInterface;
using EngagementService.Application.Dtos.Indentity;
using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User;
using MediatR;

namespace EngagementService.API.Services.Identity
{
    public class IdentityServiceClient(UserGrpc.UserGrpcClient client) : IIdentityServiceClient
    {
        public async Task<UserSimpleResp> GetUserIdByEmail(EngagementService.Application.Dtos.Indentity.GetUserIdByEmailRequest request)
        {
            var response = await client.GetUserIdByEmailAsync(new IdentityService.Grpc.Protos.User.GetUserIdByEmailRequest { Email = request.Email });
            return new UserSimpleResp { UserIDNameResp = response.UserRes.Select(u => new UserIDNameResp { UserId = Guid.Parse(u.Id), UserName = u.Name }).ToList() };
        }

        public async Task<UserSimpleResp> GetUserIdByPhone(EngagementService.Application.Dtos.Indentity.GetUserIdByPhoneRequest request)
        {
            var response = await client.GetUserIdByPhoneAsync(new IdentityService.Grpc.Protos.User.GetUserIdByPhoneRequest { Phone = request.PhoneNumber });
            return new UserSimpleResp { UserIDNameResp = response.UserRes.Select(u => new UserIDNameResp { UserId = Guid.Parse(u.Id), UserName = u.Name }).ToList() };
        }
    }
}
