using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Roles.Queries.GetPermissionsByRoleId
{
    public class GetPermissionsByRoleIdHandler : IRequestHandler<GetPermissionsByRoleIdQuery, IEnumerable<RolePermissionGroupRes>>
    {
        private readonly IRoleRepository _roleRepo;

        public GetPermissionsByRoleIdHandler(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<IEnumerable<RolePermissionGroupRes>> Handle(GetPermissionsByRoleIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleRepo.GetByIdDetailsAsync(request.Id)
                ?? throw new KeyNotFoundException("Không tìm thấy role này");

            var grouped = role.RolePermissions
                .GroupBy(rp => rp.Permission.Resource)
                .Select(rp => new RolePermissionGroupRes
                {
                    Resource = rp.Key,
                    Permissions = rp.Select(p => new RolePermissionRes
                    {
                        Id = p.Permission.Id,
                        Slug = p.Permission.Slug,
                        Name = p.Permission.Name,
                        Description = p.Permission.Description ?? "",
                        IsActived = p.IsActived
                    }).ToList()
                })
                .ToList();

            return grouped;
        }
    }
}