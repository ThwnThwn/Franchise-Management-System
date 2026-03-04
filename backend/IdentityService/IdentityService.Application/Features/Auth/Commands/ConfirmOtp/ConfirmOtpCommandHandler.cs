using AutoMapper;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System.Text.Json;

namespace IdentityService.Application.Features.Auth.Commands.ConfirmOtp
{
    public class ConfirmOtpCommandHandler : IRequestHandler<ConfirmOtpCommand, Unit>
    {
        private readonly IOtpStore _otpStore;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IKafkaProducer _kafkaProducer;

        public ConfirmOtpCommandHandler(
            IOtpStore otpStore,
            IUserRepository userRepository,
            IMapper mapper,
            IKafkaProducer kafkaProducer)
        {
            _otpStore = otpStore;
            _userRepository = userRepository;
            _mapper = mapper;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<Unit> Handle(ConfirmOtpCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var register = "register";
            // ATTEMPT LIMITING: Check if too many failed attempts
            var attemptCount = await _otpStore.GetOtpAttemptCountAsync(request.Email, register);
            if (attemptCount >= 5)
            {
                // Invalidate OTP after too many attempts
                await _otpStore.RemoveOTPAsync(request.Email, register);
                await _otpStore.RemoveRegistrationDataAsync(request.Email);
                await _otpStore.ResetOtpAttemptAsync(request.Email, register);
                throw new BusinessException("Too many failed attempts. Please request a new OTP.");
            }

            // Verify OTP
            var storedOtp = await _otpStore.GetOtpAsync(request.Email, register);
            if (storedOtp == null || storedOtp != request.Otp)
            {
                // Increment attempt count on failure
                await _otpStore.IncrementOtpAttemptAsync(request.Email, register);
                var remainingAttempts = 5 - (attemptCount + 1);
                throw new BusinessException(
                    $"Invalid or expired OTP. {remainingAttempts} attempt(s) remaining.");
            }

            // Get registration data from Redis
            var registrationDataJson = await _otpStore.GetRegistrationDataAsync(request.Email);
            if (string.IsNullOrEmpty(registrationDataJson))
                throw new BusinessException("Registration data not found or expired");

            var registrationData = JsonSerializer.Deserialize<User>(registrationDataJson);
            if (registrationData == null)
                throw new BusinessException("Invalid registration data");

            // Check if user already exists (double check)
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new ConflictDuplicateException($"Email '{request.Email}' already exists");

            // Create user
            var user = new User
            {
                Email = registrationData.Email,
                Password = registrationData.Password, //hashpassword
                Phone = registrationData.Phone,
                Username = registrationData.Username,
                FirstName = registrationData.FirstName,
                LastName = registrationData.LastName,
                Gender = registrationData.Gender,
                DateOfBirth = registrationData.DateOfBirth,
                Address = registrationData.Address,
                RoleId = registrationData.RoleId, // default role: User
                IsBlocked = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _userRepository.AddWithRoleAsync(user);

            // Clean up OTP, registration data, and attempt count from Redis
            await _otpStore.RemoveOTPAsync(request.Email, register);
            await _otpStore.RemoveRegistrationDataAsync(request.Email);
            await _otpStore.ResetOtpAttemptAsync(request.Email, register);
            await _otpStore.RemoveLastOtpRequestTimeAsync(request.Email, register);

            
            //Create Loyalty Tier Account Through Kafka
            await _kafkaProducer.PublishAsync(KafkaTopics.LoyaltyTierAccount, new UserRegisteredEvent
            {
                CustomerId = user.Id
            });
            //Create Wallet Account through kafka
            await _kafkaProducer.PublishAsync(KafkaTopics.WalletAccount, new UserRegisteredEvent
            {
                CustomerId = user.Id
            });
            return Unit.Value;
        }
    }
}
