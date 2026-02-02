using System.Security.Cryptography;
using System.Text.Json;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.Auth.Response;
using IdentityService.Application.ExternalServices;
using SharedKernel.Infrastructure.Kafka;
using SharedKernel.Constants;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;
using SharedKernel.Events;

namespace IdentityService.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterRep>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IOtpStore _otpStore;
    private readonly IEmailService _emailService;
    private readonly IKafkaProducer _kafkaProducer;

    public RegisterCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IOtpStore otpStore, IEmailService emailService, IKafkaProducer kafkaProducer)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _otpStore = otpStore;
        _emailService = emailService;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<RegisterRep> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var register = "register";
        // RATE LIMITING: Check if OTP was sent recently (within 5 minutes)
        var lastOtpTime = await _otpStore.GetLastOtpRequestTimeAsync(request.Email, register);
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

        //Check mail
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictDuplicateException($"Email '{request.Email}' already exists");

        //Check username
        existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            throw new ConflictDuplicateException($"Username '{request.Username}' already exists");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        Role? role = await _roleRepository.GetByNameAsync("Customer");
        if (role == null)
            throw new ForbidenException(Message.RoleMessage.NotFound);

        var registrationData = new User
        {
            Email = request.Email,
            Password = hashedPassword,
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Gender = request.Gender,
            DateOfBirth = request.DateOrBirth,
            Phone = request.Phone,
            Address = request.Address,
            RoleId = role.Id,
            IsBlocked = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var registrationDataJson = JsonSerializer.Serialize(registrationData);

        await _otpStore.SaveRegistrationDataAsync(request.Email, registrationDataJson);

        await _otpStore.SaveOTPAsyns(request.Email, otp, register);

        // Save the time when OTP was sent for rate limiting
        await _otpStore.SaveLastOtpRequestTimeAsync(request.Email, DateTimeOffset.UtcNow, register);

        // Reset attempt count when new OTP is sent
        await _otpStore.ResetOtpAttemptAsync(request.Email, register);
        await _otpStore.RemoveLastOtpRequestTimeAsync(request.Email, register);

        // Publish OTP event to Kafka instead of sending email directly
        await _kafkaProducer.PublishAsync(KafkaTopics.OtpNotifications, new OtpRequestedEvent
        {
            Email = request.Email,
            OtpCode = otp,
            RequestedAt = DateTimeOffset.UtcNow
        });

        return new RegisterRep();
    }
}