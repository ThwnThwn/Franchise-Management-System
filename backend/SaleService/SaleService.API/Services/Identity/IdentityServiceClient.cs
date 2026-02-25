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
            : $"https://{rawUrl}";

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
                } : null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling IdentityService ({_identityServiceUrl}): {ex.Message}");
            return null; // Return null instead of throwing - order can continue without email
        }
    }

   

}