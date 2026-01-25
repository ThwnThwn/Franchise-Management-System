using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.Auth.Response;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterRep>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public RegisterCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<RegisterRep> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // check mail
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictDuplicateException($"Email '{request.Email}' already exists");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        Role? role = await _roleRepository.GetByNameAsync("User");
        if (role == null)
            throw new ForbidenException(Message.RoleMessage.NotFound);

        var user = new User
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

        await _userRepository.AddWithRoleAsync(user);

        return new RegisterRep();
    }
}
