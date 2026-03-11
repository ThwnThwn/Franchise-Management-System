using AutoMapper;
using IdentityService.Application.Abstrations;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
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
    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ConflictDuplicateException">
    /// Email '{request.Email}' đã tồn tại
    /// or
    /// Username '{request.Username}' đã tồn tại
    /// or
    /// Số điện thoại '{request.Phone}' đã tồn tại
    /// </exception>
    /// <exception cref="Exception">Không tìm thấy vai trò '{request.RoleSlug}'</exception>
    /// <exception cref="BusinessException"></exception>
    public async Task<UserRep?> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        //Fix duplicate email check (allow same email for same user)
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null && existingUser.Id != command.Id)
            throw new ConflictDuplicateException($"Email '{request.Email}' đã tồn tại");

        existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null && existingUser.Id != command.Id)
            throw new ConflictDuplicateException($"Username '{request.Username}' đã tồn tại");

        existingUser = await _userRepository.GetByPhoneAsync(request.Phone);
        if (existingUser != null && existingUser.Id != command.Id)
            throw new ConflictDuplicateException($"Số điện thoại '{request.Phone}' đã tồn tại");

        if (!string.IsNullOrEmpty(request.RoleSlug))
        {
            _ = await _roleRepository.GetBySlugAsync(request.RoleSlug)
                ?? throw new Exception($"Không tìm thấy vai trò '{request.RoleSlug}'");
        }

        var user = await _userRepository.GetByIdAsync(command.Id)
            ?? throw new BusinessException(Message.UserMessage.NotFound);
        _mapper.Map(request, user);

        await _userRepository.UpdateAsync(user);

        return _mapper.Map<UserRep>(user);
    }
}