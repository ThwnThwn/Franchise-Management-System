using AutoMapper;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.AdminCreate;

public class AdminCreateUserCommandHandler
    : IRequestHandler<AdminCreateUserCommand, UserRep>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AdminCreateUserCommandHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserRep> Handle(AdminCreateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictDuplicateException(Message.UserMessage.ExistingUser);

        var user = _mapper.Map<User>(request);
        user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

        user.CreatedAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.IsBlocked ??= false;

        await _userRepository.AddWithRoleAsync(user);

        return _mapper.Map<UserRep>(user);
    }
}
