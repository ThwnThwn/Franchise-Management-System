using IdentityService.Application.AppExceptions;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using MediatR;
using System.Security.Cryptography;
using System.Text.Json;

namespace IdentityService.Application.Features.Auth.Commands.ForgetPassword;

public class ForgetPasswordCommandHandler
    : IRequestHandler<ForgetPasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpStore _otpStore;
    private readonly IEmailService _emailService;

    public ForgetPasswordCommandHandler(
        IUserRepository userRepository,
        IOtpStore otpStore,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _otpStore = otpStore;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(ForgetPasswordCommand command, CancellationToken cancellationToken)
    {
        var forgetPassword = "forget-password";
        var request = command.Request;
        var lastOtpTime = await _otpStore.GetLastOtpRequestTimeAsync(request.Email, forgetPassword);
        if (lastOtpTime.HasValue)
        {
            var timeSinceLastRequest = DateTimeOffset.UtcNow - lastOtpTime.Value;
            var waitTime = TimeSpan.FromMinutes(5) - timeSinceLastRequest;

            if (waitTime > TimeSpan.Zero)
            {
                throw new BusinessException(
                    $"Please wait {Math.Ceiling(waitTime.TotalMinutes)} minute(s) before requesting a new OTP.");
            }
        }

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new ForbidenException($"Email '{request.Email}' does not exist");

        var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        var forgetPasswordData = new ForgetPasswordReq
        {
            Email = request.Email
        };

        var forgetPasswordJson = JsonSerializer.Serialize(forgetPasswordData);
        await _otpStore.SaveForgetPasswordDataAsync(request.Email, forgetPasswordJson);

        await _otpStore.SaveOTPAsyns(request.Email, otp, forgetPassword);
        await _otpStore.SaveLastOtpRequestTimeAsync(request.Email, DateTimeOffset.UtcNow, forgetPassword);
        await _otpStore.ResetOtpAttemptAsync(request.Email, forgetPassword);
        await _emailService.SendOtpEmailAsync(request.Email, otp);

        return Unit.Value;
    }
}
