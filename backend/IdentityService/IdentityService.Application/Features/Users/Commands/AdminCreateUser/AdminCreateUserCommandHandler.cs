using AutoMapper;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Commands.AdminCreateUser;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.AdminCreateUser;

public class AdminCreateUserCommandHandler
    : IRequestHandler<AdminCreateUserCommand, UserRep>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public AdminCreateUserCommandHandler(IUserRepository userRepository, IMapper mapper, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _roleRepository = roleRepository;
    }

    public async Task<UserRep> Handle(AdminCreateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Check email
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictDuplicateException(Message.UserMessage.ExistingUser);
        //Check phone 
        existingUser = await _userRepository.GetByPhoneAsync(request.Phone);
        if (existingUser != null)
            throw new ConflictDuplicateException($"Username '{request.Username}' already exists");
        //Check username
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
        
        if(request.CurrentHourlyWage != null)
        {
            user.CurrentHourlyWage = request.CurrentHourlyWage;
        }
        if(request.HomeStoreId != null)
        {
            user.HomeStoreId = request.HomeStoreId;
        }

        await _userRepository.AddWithRoleAsync(user);

        return _mapper.Map<UserRep>(user);
    }
}
