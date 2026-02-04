using MediatR;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Persistences.Caches;

namespace IdentityService.Application.Features.Roles.Queries.GetPermissionsBySlug
{
    public class GetPermissionsBySlugHandler : IRequestHandler<GetPermissionsBySlugQuery, IEnumerable<string>>
    {
        private readonly IPermissionRepository _permRepo; // Sửa lại dùng PermRepo
        private readonly IPermissionCache _permCache;

        public GetPermissionsBySlugHandler(IPermissionRepository permRepo, IPermissionCache permCache)
        {
            _permRepo = permRepo;
            _permCache = permCache;
        }

        public async Task<IEnumerable<string>> Handle(GetPermissionsBySlugQuery request, CancellationToken cancellationToken)
        {
            // 1. Lấy dữ liệu từ DB
            var permissions = await _permRepo.GetSlugsByRoleSlugAsync(request.RoleSlug, true);

            // 2. Logic Cache (Side Effect)
            // Nếu tìm thấy hoặc list rỗng cũng nên cache để tránh Cache Penetration
            if (permissions != null)
            {
                // Ghi ngược vào Redis để lần sau Gateway tự đọc
                await _permCache.SetRolePermissionsAsync(request.RoleSlug, permissions);
            }

            // 3. Trả về kết quả (để gRPC trả về cho Gateway dùng ngay)
            return permissions ?? [];
        }
    }
}