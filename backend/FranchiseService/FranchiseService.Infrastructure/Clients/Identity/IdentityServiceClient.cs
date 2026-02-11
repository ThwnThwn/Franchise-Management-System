using Grpc.Core;
using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User;
using FranchiseService.Application.ClientInterfaces;
using Microsoft.Extensions.Configuration;

namespace FranchiseService.Infrastructure.Clients.Identity
{
    /// <summary>
    /// gRPC Client giao tiếp với IdentityService.
    /// Dùng để validate user và lấy thông tin nhân viên (username, email).
    /// </summary>
    public class IdentityServiceClient : IIdentityServiceClient
    {
        private readonly UserGrpc.UserGrpcClient _client;

        public IdentityServiceClient(IConfiguration configuration)
        {
            var rawUrl = configuration["GrpcSettings:IdentityServiceUrl"] ?? "https://localhost:5001";

            var identityServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                     rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? rawUrl
                : $"https://{rawUrl}";

            // Cho phép self-signed certificate trong môi trường dev
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress(identityServiceUrl, new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

            _client = new UserGrpc.UserGrpcClient(channel);
        }

        /// <summary>
        /// Lấy thông tin user theo ID từ IdentityService.
        /// Trả về null nếu không tìm thấy (thay vì throw).
        /// </summary>
        public async Task<UserInfo?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var request = new GetUserByIdRequest { Id = userId.ToString() };
                var response = await _client.GetByIdAsync(request);

                if (response == null) return null;

                return new UserInfo
                {
                    Id = Guid.Parse(response.Id),
                    Username = response.Username,
                    Email = response.Email,
                    Phone = response.Phone,
                    CurrentHourlyWage = response.HasCurrentHourlyWage ? (decimal)response.CurrentHourlyWage : null
                };
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                // User không tồn tại → trả null
                return null;
            }
        }
    }
}
