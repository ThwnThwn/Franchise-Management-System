using IdentityService.Application.Dtos.Permission;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Permission.Queries.GetAllGrouped
{
    public class GetPermissionsByIdHandler : IRequestHandler<GetPermissionsByIdQuery, IEnumerable<PermissionGroupDto>>
    {
        private readonly IPermissionRepository _permRepo;

        public GetPermissionsByIdHandler(IPermissionRepository permRepo)
        {
            _permRepo = permRepo;
        }

        public async Task<IEnumerable<PermissionGroupDto>> Handle(GetPermissionsByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Lấy tất cả (Flat List)
            var allPerms = await _permRepo.GetAllAsync();

            // 2. Group By Resource (In-Memory grouping)
            var grouped = allPerms
                .GroupBy(p => p.Resource)
                .Select(g => new PermissionGroupDto
                {
                    Resource = g.Key,
                    Permissions = g.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Slug = p.Slug,
                        Name = p.Name,
                        Description = p.Description ?? ""
                    }).ToList()
                })
                .ToList();

            return grouped;
        }
    }
}