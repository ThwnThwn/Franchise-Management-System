using IdentityService.Application.Abstrations;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;

namespace IdentityService.Application
{
    public class PermissionSyncService : IPermissionSyncService
    {
        private readonly IRoleRepository _roleRepo;
        private readonly IPermissionRepository _permRepo;
        private readonly IPermissionCache _permCache;

        public PermissionSyncService(
            IRoleRepository roleRepo,
            IPermissionRepository permRepo,
            IPermissionCache permCache
        )
        {
            _roleRepo = roleRepo;
            _permRepo = permRepo;
            _permCache = permCache;
        }

        public async Task SyncAllAsync()
        {
            // 1. Lấy Code (Template/Default)
            var codePermissions = AppPermissions.AllPermissions;

            // 2. Lấy DB (Real Data)
            var dbPermissions = await _permRepo.GetAllAsync();

            // --- XỬ LÝ: THÊM MỚI (Chỉ thêm nếu chưa có) ---
            var toAdd = new List<Permission>();
            var toUpdate = new List<Permission>();

            foreach (var codeP in codePermissions)
            {
                var dbP = dbPermissions.FirstOrDefault(p => p.Slug == codeP.Slug);

                if (dbP == null)
                {
                    toAdd.Add(new Permission
                    {
                        Slug = codeP.Slug,
                        Name = codeP.Name,        // Giá trị mặc định
                        Description = codeP.Description, // Giá trị mặc định
                        Resource = codeP.Resource
                    });
                }
                else
                {
                    // Case 2: Đã có trong DB -> TÔN TRỌNG DB, chỉ CẬP NHẬT Resource nếu khác
                    if (dbP.Resource != codeP.Resource)
                    {
                        dbP.Resource = codeP.Resource;
                        toUpdate.Add(dbP);
                    }
                }
            }

            // --- XỬ LÝ: XÓA (Giữ nguyên logic này để dọn rác) ---
            // Nếu Code đã xóa Slug đó đi rồi (nghĩa là tính năng đã bỏ), thì nên xóa trong DB
            var validSlugs = codePermissions.Select(c => c.Slug).ToHashSet();
            var toDeleteIds = dbPermissions
                .Where(p => !validSlugs.Contains(p.Slug))
                .Select(p => p.Id)
                .ToList();

            // --- THỰC THI DB ---
            if (toAdd.Count != 0) await _permRepo.AddRangeAsync(toAdd);
            if (toUpdate.Count != 0) await _permRepo.UpdateRangeAsync(toUpdate); // Chỉ update Resource thôi
            if (toDeleteIds.Count != 0) await _permRepo.DeleteRangeAsync(toDeleteIds);

            // --- THỰC THI CACHE ---
            // 1. Xóa sạch cache cũ
            await _permCache.ClearAllRolesCacheAsync();

            // 2. Load Role kèm Permissions từ DB
            var allRoles = await _roleRepo.GetAllDetailsAsync();

            // 3. Cache từng Role theo Slug
            var syncTasks = allRoles.Select(role =>
            {
                // Giả sử Entity Role có field Slug (ví dụ: "admin", "hr_manager")
                if (string.IsNullOrEmpty(role.Slug)) return Task.CompletedTask;

                var permissionSlugs = role.RolePermissions
                                .Select(rp => rp.Permission.Slug)
                                .ToList();

                // Gọi hàm Cache với Slug
                return _permCache.SetRolePermissionsAsync(role.Slug, permissionSlugs);
            });

            await Task.WhenAll(syncTasks);
        }
    }
}