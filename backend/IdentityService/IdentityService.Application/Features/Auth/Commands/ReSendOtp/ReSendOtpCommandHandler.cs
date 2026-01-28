using Application.Persistences.Caches;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.ExternalServices;
using MediatR;
using System.Security.Cryptography;

namespace IdentityService.Application.Features.Auth.Commands.ReSendOtp
{
    public class ReSendOtpCommandHandler : IRequestHandler<ReSendOtpCommand, Unit>
    {
        private readonly IOtpStore _otpStore;
        private readonly IEmailService _emailService;

        public ReSendOtpCommandHandler(IOtpStore otpStore, IEmailService emailService)
        {
            _otpStore = otpStore;
            _emailService = emailService;
        }
        public async Task<Unit> Handle(ReSendOtpCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            // RATE LIMITING: Check if OTP was sent recently (within 5 minutes)
            var lastOtpTime = await _otpStore.GetLastOtpRequestTimeAsync(request.Email, request.Purpose);
            if (lastOtpTime.HasValue)
            {
                var timeSinceLastRequest = DateTimeOffset.UtcNow - lastOtpTime.Value;
                var waitTime = TimeSpan.FromSeconds(30) - timeSinceLastRequest;

                if (waitTime > TimeSpan.Zero)
                {
                    throw new BusinessException(
                        "Please wait 30 seconds before resending OTP.");
                }
            }
            var storedOtp = await _otpStore.GetOtpAsync(request.Email, request.Purpose);
            if (storedOtp == null)
            {
                throw new BusinessException("Otp data not found or expired");
            }
            await _otpStore.RemoveOTPAsync(request.Email, request.Purpose);
            //New otp
            var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            await _otpStore.SaveOTPAsyns(request.Email, otp, request.Purpose);
            // Save the time when OTP was sent for rate limiting
            await _otpStore.SaveLastOtpRequestTimeAsync(request.Email, DateTimeOffset.UtcNow, request.Purpose);
            // Reset attempt count when new OTP is sent
            await _otpStore.ResetOtpAttemptAsync(request.Email, request.Purpose);
            await _emailService.SendOtpEmailAsync(request.Email, otp);

            return Unit.Value;
        }
    }
}
