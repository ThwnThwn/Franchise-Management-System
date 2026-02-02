using IdentityService.Application.AppExceptions;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Dtos.Auth.Response;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using MediatR;
using System.Text.Json;

namespace IdentityService.Application.Features.Auth.Commands.ConfirmNewPassword;

public class ConfirmNewPasswordCommandHandler
    : IRequestHandler<ConfirmNewPasswordCommand, ConfirmOtpForgetPasswordOtpRep>
{
    private readonly IOtpStore _otpStore;
    private readonly IUserRepository _userRepository;
    private readonly JwtHelper _jwtHelper;

    public ConfirmNewPasswordCommandHandler(
        IOtpStore otpStore,
        IUserRepository userRepository,
        JwtHelper jwtHelper)
    {
        _otpStore = otpStore;
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
    }

    public async Task<ConfirmOtpForgetPasswordOtpRep> Handle(ConfirmNewPasswordCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var forgetPassword = "forget-password";
        var attemptCount = await _otpStore.GetOtpAttemptCountAsync(request.Email, forgetPassword);

        if (attemptCount >= 5)
        {
            await _otpStore.RemoveOTPAsync(request.Email, forgetPassword);
            await _otpStore.RemoveForgetPasswordDataAsync(request.Email);
            await _otpStore.ResetOtpAttemptAsync(request.Email, forgetPassword);

            throw new BusinessException("Too many failed attempts. Please request a new OTP.");
        }

        var storedOtp = await _otpStore.GetOtpAsync(request.Email, forgetPassword);

        if (storedOtp == null || storedOtp != request.Otp)
        {
            await _otpStore.IncrementOtpAttemptAsync(request.Email, forgetPassword);

            var remainingAttempts = 5 - (attemptCount + 1);

            throw new BusinessException(
                $"Invalid or expired OTP. {remainingAttempts} attempt(s) remaining.");
        }

        var forgetPasswordJson = await _otpStore.GetForgetPasswordAsync(request.Email);

        if (string.IsNullOrEmpty(forgetPasswordJson))
            throw new BusinessException("Forget password session expired. Please request OTP again.");

        var forgetPasswordData =
            JsonSerializer.Deserialize<ForgetPasswordReq>(forgetPasswordJson);

        if (forgetPasswordData == null)
            throw new BusinessException("Invalid forget password ");

        var accessToken = _jwtHelper.GenerateForgetPasswordToken(request.Email);

        await _otpStore.RemoveOTPAsync(request.Email, forgetPassword);
        await _otpStore.RemoveForgetPasswordDataAsync(request.Email);
        await _otpStore.ResetOtpAttemptAsync(request.Email, forgetPassword);
        await _otpStore.RemoveLastOtpRequestTimeAsync(request.Email, forgetPassword);
        return new ConfirmOtpForgetPasswordOtpRep
        {
            AccessForgetPasswordToken = accessToken,
        };
    }
}