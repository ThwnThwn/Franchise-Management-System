using Grpc.Core;
using IdentityService.Application.Abstrations;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Grpc.Protos.Auth;
using IdentityService.Grpc.Protos.User;

namespace IdentityService.Grpc.Services;

public class AuthGrpcService : AuthGrpc.AuthGrpcBase
{
    private readonly IAuthService _authService;

    public AuthGrpcService(IAuthService authService)
    {
        _authService = authService;
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        try
        {
            var rep = await _authService.LoginAsync(new LoginReq
            {
                Email = request.Email,
                Password = request.Password
            });
            //Lấy từ proto
            return new LoginResponse
            {
                AccessToken = rep.AccessToken,
                RefreshToken = rep.RefreshToken,
                User = MapUser(rep.User)
            };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
        }
    }

    public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        try
        {
            var rep = await _authService.RegisterAsync(new RegisterReq
            {
                Email = request.Email,
                Password = request.Password,
                Phone = request.Phone,
                Address = request.Address
            });
            //Lấy từ proto
            return new RegisterResponse
            {
                Message = rep.Message
            };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
    {
        try
        {
            var rep = await _authService.RefreshTokenAsync(request.RefreshToken);
            //Lấy từ proto
            return new RefreshTokenResponse
            {
                AccessToken = rep.AccessToken,
                RefreshToken = rep.RefreshToken
            };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
        }
    }

    public override async Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
    {
        try
        {
            var ok = await _authService.LogoutAsync(
                refreshToken: request.RefreshToken,
                accessToken: string.IsNullOrWhiteSpace(request.AccessToken) ? null : request.AccessToken
            );
            //Lấy từ proto
            return new LogoutResponse { Success = ok };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    private static UserResponse MapUser(UserRep user)
    {
        return new UserResponse
        {
            Id = user.Id.ToString() ?? "",
            Email = user.Email ?? "",
            Phone = user.Phone ?? "",
            Address = user.Address ?? "",
            Role = user.Role == null
                ? new IdentityService.Grpc.Protos.Role.RoleUserResponse()
                : new IdentityService.Grpc.Protos.Role.RoleUserResponse
                {
                    Id = user.Role.Id.ToString(),
                    Name = user.Role.Name ?? ""
                }
        };
    }
}
