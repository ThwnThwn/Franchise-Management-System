using AutoMapper;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleRep?>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public UpdateRoleCommandHandler(IRoleRepository roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<RoleRep?> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var existingRole = await _roleRepository.GetByNameAsync(request.Name);
        if (existingRole != null)
            throw new Exception($"Role '{request.Name}' already exists");

        var role = await _roleRepository.GetByIdAsync(command.Id);
        if (role == null) return null;

        _mapper.Map(request, role);
        role.UpdatedAt = DateTimeOffset.UtcNow;

        await _roleRepository.UpdateAsync(role);

        return _mapper.Map<RoleRep>(role);
    }
}
