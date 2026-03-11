using AutoMapper;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleRep>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public CreateRoleCommandHandler(IRoleRepository roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }
    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="DbUpdateException">Role '{request.Name}' already exists</exception>
    public async Task<RoleRep> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var existingRole = await _roleRepository.GetByNameAsync(request.Name);
        if (existingRole != null)
            throw new DbUpdateException($"Role '{request.Name}' already exists");

        var role = _mapper.Map<Role>(request);
        role.CreatedAt = DateTimeOffset.UtcNow;
        role.UpdatedAt = DateTimeOffset.UtcNow;

        await _roleRepository.AddAsync(role);

        return _mapper.Map<RoleRep>(role);
    }
}
