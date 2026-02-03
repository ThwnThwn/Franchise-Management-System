using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Features.Roles.Commands.UpdateRolePermissions
{
    public class UpdateRolePermissionsHandler : IRequestHandler<UpdateRolePermissionsCommand> // Return Unit
    {
        private readonly IRoleRepository _roleRepo;
        private readonly IPermissionRepository _permRepo;
        private readonly IPermissionCache _permCache;

        public UpdateRolePermissionsHandler(IRoleRepository roleRepo,
            IPermissionRepository permRepo,
            IPermissionCache permCache)
        {
            _roleRepo = roleRepo;
            _permRepo = permRepo;
            _permCache = permCache;
        }

        public async Task<Unit> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            // 1. Lấy Role kèm Permissions hiện tại
            var role = await _roleRepo.GetByIdDetailsAsync(request.Id)
                 ?? throw new KeyNotFoundException($"Không tìm thấy role {request.Id}");

            // 2. Gom tất cả slug cần xử lý (cả grant và revoke) để query DB 1 lần
            var allSlugs = request.PermissionsToGrant.Concat(request.PermissionsToRevoke).Distinct().ToList();

            // Lấy Entity từ DB lên
            var relevantPermissions = await _permRepo.GetListBySlugsAsync(allSlugs);

            // 3. Vòng lặp xử lý Logic
            foreach (var perm in relevantPermissions)
            {
                // Tìm xem role đã có link tới perm này chưa
                var existingLink = role.RolePermissions
                    .FirstOrDefault(rp => rp.PermissionId == perm.Id);

                // A. Xử lý GRANT (Cấp quyền)
                if (request.PermissionsToGrant.Contains(perm.Slug))
                {
                    if (existingLink == null)
                    {
                        // Chưa có -> Thêm mới (Active)
                        role.RolePermissions.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = perm.Id,
                            IsActived = true
                        });
                    }
                    else if (!existingLink.IsActived)
                    {
                        // Có rồi mà đang tắt -> Bật lên
                        existingLink.IsActived = true;
                    }
                }

                // B. Xử lý REVOKE (Thu hồi)
                // Lưu ý: Dùng 'else if' để ưu tiên Grant nếu lỡ FE gửi trùng slug trong cả 2 mảng (Safety first)
                else if (request.PermissionsToRevoke.Contains(perm.Slug))
                {
                    if (existingLink != null && existingLink.IsActived)
                    {
                        // Đang bật -> Tắt đi
                        existingLink.IsActived = false;
                    }
                }
            }

            //await _permRepo.UpdateRangeAsync(relevantPermissions);
            await _roleRepo.UpdateAsync(role);

            // 4. Update Cache (Chỉ cache những cái đang Active)
            var activePerms = await _permRepo.GetSlugsByRoleSlugAsync(role.Slug, true);
            await _permCache.SetRolePermissionsAsync(role.Slug, activePerms);

            return Unit.Value;
        }
    }
}