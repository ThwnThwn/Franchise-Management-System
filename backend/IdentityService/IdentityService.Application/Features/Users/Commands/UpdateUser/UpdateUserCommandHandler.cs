using AutoMapper;
using IdentityService.Application.Abstrations;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Security.Principal;

namespace IdentityService.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserRep?>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(IUserRepository userRepository, IMapper mapper, ICurrentUserService currentUser, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _currentUser = currentUser;
        _roleRepository = roleRepository;
    }

    public async Task<UserRep?> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null)
            throw new UnauthorizedAccessException("Unauthenticated");
        var currentUser = await _userRepository.GetByIdAsync(currentUserId.Value);
        if (currentUser == null)
            throw new UnauthorizedAccessException("Unauthenticated");
        var roleAdmin = await _roleRepository.GetByNameAsync("Admin");

        //Only allow update own profile
        if (currentUserId.Value != command.Id && currentUser.Role != roleAdmin)
            throw new ForbidenException("You can only update your own profile");

        var request = command.Request;

        //Fix duplicate email check (allow same email for same user)
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null && existingUser.Id != command.Id)
            throw new ConflictDuplicateException(Message.UserMessage.ExistingUser);

        var user = await _userRepository.GetByIdAsync(command.Id);
        if (user == null)
            return null;

        _mapper.Map(request, user);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _userRepository.UpdateAsync(user);

        return _mapper.Map<UserRep>(user);
    }
}
