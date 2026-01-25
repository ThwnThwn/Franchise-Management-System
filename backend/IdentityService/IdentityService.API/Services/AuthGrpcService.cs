using Grpc.Core;
using IdentityService.Application.Features.Auth.Commands.ConfirmOtp;
using IdentityService.Application.Features.Auth.Commands.Login;
using IdentityService.Application.Features.Auth.Commands.Logout;
using IdentityService.Application.Features.Auth.Commands.Register;
using IdentityService.Application.Features.Auth.Commands.TokenRefresh;
using IdentityService.Grpc.Protos.Auth;
using IdentityService.Grpc.Protos.User;
using MediatR;

namespace IdentityService.API.Services;

public class AuthGrpcService : AuthGrpc.AuthGrpcBase
{
    private readonly IMediator _mediator;

    public AuthGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        try
        {
            var rep = await _mediator.Send(new LoginCommand(new()
            {
                Email = request.Email,
                Password = request.Password
            }));

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
            var rep = await _mediator.Send(new RegisterCommand(new()
            {
                Email = request.Email,
                Password = request.Password,
                Phone = request.Phone,
                Address = request.Address
            }));

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
            var rep = await _mediator.Send(new TokenRefreshCommand(request.RefreshToken));

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
            var ok = await _mediator.Send(new LogoutCommand(
                RefreshToken: request.RefreshToken,
                AccessToken: string.IsNullOrWhiteSpace(request.AccessToken) ? null : request.AccessToken
            ));

            return new LogoutResponse { Success = ok };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<ConfirmOtpResponse> ConfirmOtp(
            ConfirmOtpRequest request,
            ServerCallContext context)
    {
        await _mediator.Send(new ConfirmOtpCommand( new()
        {   Email = request.Email, 
            Otp = request.Otp 
        }));

        return new ConfirmOtpResponse
        {
            Success = true,
            Message = "Confirm OTP successfully"
        };
    }

    private static UserResponse MapUser(Application.Dtos.User.Respone.UserRep user)
    {
        return new UserResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? "",
            Phone = user.Phone ?? "",
            Address = user.Address ?? "",
            Role = user.Role == null
                ? new Grpc.Protos.Role.RoleUserResponse()
                : new Grpc.Protos.Role.RoleUserResponse
                {
                    Id = user.Role.Id.ToString(),
                    Name = user.Role.Name ?? ""
                }
        };
    }
}
