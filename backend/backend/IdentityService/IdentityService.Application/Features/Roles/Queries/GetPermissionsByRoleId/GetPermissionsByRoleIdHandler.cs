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
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// Response from the request
        /// </returns>
        /// <exception cref="KeyNotFoundException">Không tìm thấy role này</exception>
        public async Task<IEnumerable<RolePermissionGroupRes>> Handle(GetPermissionsByRoleIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleRepo.GetByIdDetailsAsync(request.Id)
                ?? throw new KeyNotFoundException("Không tìm thấy role này");

            var grouped = role.Permissions
                .GroupBy(rp => rp.Resource)
                .Select(rp => new RolePermissionGroupRes
                {
                    Resource = rp.Key,
                    Permissions = rp.Select(p => new RolePermissionRes
                    {
                        Id = p.Id,
                        Slug = p.Slug,
                        Name = p.Name,
                        Description = p.Description ?? "",
                        //IsActived = p.IsActived
                    }).ToList()
                })
                .ToList();

            return grouped;
        }
    }
}