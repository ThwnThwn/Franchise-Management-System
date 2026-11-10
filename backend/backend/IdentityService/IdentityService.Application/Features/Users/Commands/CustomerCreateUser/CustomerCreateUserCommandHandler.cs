using AutoMapper;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.CustomerCreateUser;

public class CustomerCreateUserCommandHandler
    : IRequestHandler<CustomerCreateUserCommand, UserRep>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public CustomerCreateUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _mapper = mapper;
    }
    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ConflictDuplicateException"></exception>
    /// <exception cref="Exception">Slug" + request.RoleSlug + " not found</exception>
    public async Task<UserRep> Handle(CustomerCreateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictDuplicateException(Message.UserMessage.ExistingUser);
        existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            throw new ConflictDuplicateException(Message.UserMessage.ExistingUser);

        var user = _mapper.Map<User>(request);

        var role = await _roleRepository.GetBySlugAsync(request.RoleSlug);
        if (role == null)
            throw new Exception("Slug" + request.RoleSlug + " not found");
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("1");

        user.Password = hashedPassword;
        user.RoleId ??= role.Id;
        user.CreatedAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.IsBlocked ??= false;

        await _userRepository.AddWithRoleAsync(user);

        return _mapper.Map<UserRep>(user);
    }
}
