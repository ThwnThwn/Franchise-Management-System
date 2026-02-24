using EngagementService.Application.Persistences.Cache;
using EngagementService.Domain.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace EngagementService.Infrastucture.Persistences.Cache
{
    public class MembershipTierCache : IMembershipTierCache
    {
        private readonly IDatabase _db;

        // Index bây giờ sẽ lưu list các id của MembershipTier, để dễ dàng load tất cả vào cache
        private const string TIER_INDEX_KEY = "tier_id:index";

        public MembershipTierCache(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        // Key format: "engagement:tier:{tier}" (Ví dụ: engagement:tier:silver, engagement:tier:gold)
        private string GetKey(string tier) => $"engagement:tier:{tier}";
        // LƯU/CẬP NHẬT: Không set expiry để data tồn tại mãi mãi
        public async Task SetMembershipTierAsync(Guid tierId, MembershipTier data)
        {
            var key = GetKey(tierId.ToString());
            var jsonData = JsonSerializer.Serialize(data);

            // Dùng Transaction để đảm bảo tính toàn vẹn giữa Data và Index
            var batch = _db.CreateTransaction();

            // Lưu dữ liệu (Không truyền tham số expiry = vĩnh viễn)
            _ = batch.StringSetAsync(key, jsonData);

            // Đánh dấu ID này vào Index
            _ = batch.SetAddAsync(TIER_INDEX_KEY, tierId.ToString());

            await batch.ExecuteAsync();
        }

        // LẤY DỮ LIỆU
        public async Task<MembershipTier?> GetMembershipTierAsync(Guid tierId)
        {
            var data = await _db.StringGetAsync(GetKey(tierId.ToString()));
            return data.HasValue ? JsonSerializer.Deserialize<MembershipTier>(data!) : default;
        }

        // KIỂM TRA TỒN TẠI
        public async Task<bool> IsMembershipTierLoadedAsync(Guid tierId)
        {
            return await _db.KeyExistsAsync(GetKey(tierId.ToString()));
        }

        // XÓA 1 TIER (Khi Delete dưới DB)
        public async Task RemoveMembershipTierAsync(Guid tierId)
        {
            var key = GetKey(tierId.ToString());
            var batch = _db.CreateTransaction();

            _ = batch.KeyDeleteAsync(key);
            _ = batch.SetRemoveAsync(TIER_INDEX_KEY, tierId.ToString());

            await batch.ExecuteAsync();
        }

        // XÓA TẤT CẢ (Reset hệ thống)
        public async Task ClearAllMembershipTierCacheAsync()
        {
            var tiers = await _db.SetMembersAsync(TIER_INDEX_KEY);
            if (tiers.Length == 0) return;

            var keysToDelete = tiers.Select(id => (RedisKey)GetKey(id.ToString())).ToList();
            keysToDelete.Add(TIER_INDEX_KEY); // Xóa luôn cái Index

            await _db.KeyDeleteAsync(keysToDelete.ToArray());
        }
    }
}
