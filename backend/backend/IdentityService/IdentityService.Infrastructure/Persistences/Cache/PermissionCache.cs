using IdentityService.Application.Persistences.Caches;
using StackExchange.Redis;

namespace IdentityService.Infrastructure.Persistences.Cache
{
    public class PermissionCache : IPermissionCache
    {
        private readonly IDatabase _db;

        // Index bây giờ sẽ lưu list các Slug (string) thay vì ID
        private const string ROLE_INDEX_KEY = "role_slugs:index";

        public PermissionCache(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        // Key format: "auth:role:{slug}" (Ví dụ: auth:role:admin, auth:role:staff)
        private string GetKey(string roleSlug) => $"auth:role:{roleSlug}";

        public async Task SetRolePermissionsAsync(string roleSlug, IEnumerable<string> permissions)
        {
            var key = GetKey(roleSlug);
            var values = permissions.Select(s => (RedisValue)s).ToArray();

            var trans = _db.CreateTransaction();

            // 1. Clear cũ
            _ = trans.KeyDeleteAsync(key);

            // 2. Insert mới (nếu có quyền)
            if (values.Length > 0)
            {
                _ = trans.SetAddAsync(key, values);
                // Role Cache nên để TTL lâu (ví dụ 7 ngày) vì Role ít khi đổi
                _ = trans.KeyExpireAsync(key, TimeSpan.FromDays(7));
            }

            // 3. Update Index (Lưu slug vào sổ cái)
            _ = trans.SetAddAsync(ROLE_INDEX_KEY, roleSlug);

            await trans.ExecuteAsync();
        }

        public async Task ClearRolePermissionsCacheAsync(string roleSlug)
        {
            var key = GetKey(roleSlug);
            await _db.KeyDeleteAsync(key);
            // Không nhất thiết phải xóa khỏi Index, nhưng nếu muốn sạch thì xóa
            await _db.SetRemoveAsync(ROLE_INDEX_KEY, roleSlug);
        }

        public async Task ClearAllRolesCacheAsync()
        {
            // 1. Lấy tất cả Role Slug từ Index
            var roleSlugs = await _db.SetMembersAsync(ROLE_INDEX_KEY);
            if (roleSlugs.Length == 0) return;

            // 2. Tạo list key
            var keys = roleSlugs.Select(slug => (RedisKey)GetKey(slug.ToString())).ToArray();

            // 3. Xóa data
            await _db.KeyDeleteAsync(keys);

            // 4. Xóa Index
            await _db.KeyDeleteAsync(ROLE_INDEX_KEY);
        }

        // Hàm này Gateway sẽ dùng (qua logic check) hoặc Service dùng để check
        public async Task<bool> IsRoleLoadedAsync(string roleSlug)
        {
            return await _db.KeyExistsAsync(GetKey(roleSlug));
        }
    }
}