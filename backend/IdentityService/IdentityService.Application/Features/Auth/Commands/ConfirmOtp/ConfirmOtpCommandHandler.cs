using Application.Persistences.Caches;
using AutoMapper;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;
using System.Text.Json;

namespace IdentityService.Application.Features.Auth.Commands.ConfirmOtp
{
    public class ConfirmOtpCommandHandler : IRequestHandler<ConfirmOtpCommand, Unit>
    {
        private readonly IOtpStore _otpStore;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ConfirmOtpCommandHandler(
            IOtpStore otpStore,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _otpStore = otpStore;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(ConfirmOtpCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request; 
            // Verify OTP
            var storedOtp = await _otpStore.GetOtpAsync(request.Email);
            if (storedOtp == null || storedOtp != request.Otp)
                throw new BusinessException("Invalid or expired OTP");

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
                DateOrBirth = registrationData.DateOrBirth,
                Address = registrationData.Address,
                RoleId = registrationData.RoleId, // default role: User
                IsBlocked = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _userRepository.AddWithRoleAsync(user);

            // Clean up OTP and registration data from Redis
            await _otpStore.RemoveOTPAsync(request.Email);
            await _otpStore.RemoveRegistrationDataAsync(request.Email);

            return Unit.Value;
        }
    }
}
