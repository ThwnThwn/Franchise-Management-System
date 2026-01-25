using Application.Persistences.Caches;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.Auth.Response;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;
using System.Text.Json;

namespace IdentityService.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterRep>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IOtpStore _otpStore;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IOtpStore otpStore, IEmailService emailService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _otpStore = otpStore;
        _emailService = emailService;
    }

    public async Task<RegisterRep> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // check mail
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictDuplicateException($"Email '{request.Email}' already exists");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var random = new Random();
        var otp = random.Next(100000, 999999).ToString();

        Role? role = await _roleRepository.GetByNameAsync("User");
        if (role == null)
            throw new ForbidenException(Message.RoleMessage.NotFound);

        var registrationData = new User
        {
            Email = request.Email,
            Password = hashedPassword,
            Phone = request.Phone,
            Address = request.Address,
            RoleId = role.Id,
            IsBlocked = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var registrationDataJson = JsonSerializer.Serialize(registrationData);

        await _otpStore.SaveRegistrationDataAsync(request.Email, registrationDataJson);

        await _otpStore.SaveOTPAsyns(request.Email, otp);

        await _emailService.SendOtpEmailAsync(request.Email, otp);

        return new RegisterRep();
    }
}
