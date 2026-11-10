using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Features.Auth.Commands.ConfirmNewPassword;
using IdentityService.Application.Features.Auth.Commands.ConfirmOtp;
using IdentityService.Application.Features.Auth.Commands.ForgetPassword;
using IdentityService.Application.Features.Auth.Commands.Login;
using IdentityService.Application.Features.Auth.Commands.Logout;
using IdentityService.Application.Features.Auth.Commands.NewPassword;
using IdentityService.Application.Features.Auth.Commands.Register;
using IdentityService.Application.Features.Auth.Commands.ReSendOtp;
using IdentityService.Application.Features.Auth.Commands.TokenRefresh;
using IdentityService.Grpc.Protos.Auth;
using IdentityService.Grpc.Protos.User;
using MediatR;
using System.Globalization;

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
                EmailOrUsername = request.EmailOrUsername,
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
            var dob = DateTime.ParseExact(
                request.DateOfBirth,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture
            );

            var rep = await _mediator.Send(new RegisterCommand(new()
            {
                Email = request.Email,
                Username = request.Username,
                Password = request.Password,
                Phone = request.Phone,
                Address = request.Address,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOrBirth = dob,
                Gender = request.Gender
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

    public override async Task<ConfirmOtpResponse> ConfirmRegisterOtp(ConfirmOtpRequest request, ServerCallContext context)
    {
        await _mediator.Send(new ConfirmOtpCommand(new()
        {
            Email = request.Email,
            Otp = request.Otp
        }));

        return new ConfirmOtpResponse
        {
            Success = true,
            Message = "Confirm OTP successfully"
        };
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

    public override async Task<RegisterResponse> ForgetPassword(ForgetPasswordRequest request, ServerCallContext context)
    {
        await _mediator.Send(new ForgetPasswordCommand(new()
        {
            Email = request.Email
        }));

        return new RegisterResponse
        {
            Message = "OTP has been sent to your email"
        };
    }

    public override async Task<ConfirmOtpForgetPasswordOtpResponse> ConfirmForgetPasswordOtp(ConfirmOtpRequest request, ServerCallContext context)
    {
        var rep = await _mediator.Send(new ConfirmNewPasswordCommand(new()
        {
            Email = request.Email,
            Otp = request.Otp
        }));

        return new ConfirmOtpForgetPasswordOtpResponse
        {
            AccessForgetPasswordToken = rep.AccessForgetPasswordToken
        };
    }

    public override async Task<ConfirmOtpResponse> NewPassword(NewPasswordRequest request, ServerCallContext context)
    {
        var userEmailEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-email", StringComparison.OrdinalIgnoreCase));

        if (userEmailEntry == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-email in request headers"));
        }

        await _mediator.Send(new NewPasswordCommand(
            Email: userEmailEntry.Value,
            Request: new()
            {
                NewPassword = request.NewPassword
            }
        ));

        return new ConfirmOtpResponse
        {
            Success = true,
            Message = "Password updated successfully"
        };
    }

    public override async Task<ReSendOtpResponse> ReSendOtp(ReSendOtpRequest request, ServerCallContext context)
    {
        var command = new ReSendOtpCommand(
            new ReSendOtpReq
            {
                Email = request.Email,
                Purpose = request.Type
            }
        );
        await _mediator.Send(command);
        return new ReSendOtpResponse
        {
            Message = "OTP has been resent successfully"
        };
    }

    private static UserResponse MapUser(Application.Dtos.User.Respone.UserRep user)
    {
        return new UserResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? "",
            Phone = user.Phone ?? "",
            Username = user.Username ?? "",
            Address = user.Address ?? "",
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth.HasValue
                ? Timestamp.FromDateTime(user.DateOfBirth.Value.ToUniversalTime())
                : null,
            IsBlocked = user.IsBlocked ?? false,
            CreatedAt = Timestamp.FromDateTime(user.CreatedAt.UtcDateTime),
            UpdatedAt = Timestamp.FromDateTime(user.UpdatedAt.UtcDateTime),
            HomeStoreId = user.HomeStoreId.HasValue ? user.HomeStoreId.Value.ToString() : "",
            Role = user.Role == null
                ? new Grpc.Protos.Role.RoleUserResponse()
                : new Grpc.Protos.Role.RoleUserResponse
                {
                    Id = user.Role.Id.ToString(),
                    Name = user.Role.Name ?? "",
                    Slug = user.Role.Slug ?? ""
                }
        };
    }
}