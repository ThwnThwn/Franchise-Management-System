using EngagementService.Application.Persistences.Cache;
using EngagementService.Domain.Entities;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EngagementService.Infrastucture.Persistences.Cache
{
    public class PointRuleCache : IPointRuleCache
    {
        private readonly IDatabase _db;

        // Index bây giờ sẽ lưu list các id của PointRule, để dễ dàng load tất cả vào cache
        private const string POINT_RULE_INDEX_KEY = "point_rule_id:index";

        public PointRuleCache(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        private string GetKey(string ruleId) => $"engagement:point_rule:{ruleId}";
        public async Task ClearAllPointRuleCacheAsync()
        {
            var rules = await _db.SetMembersAsync(POINT_RULE_INDEX_KEY);
            if (rules.Length == 0) return;

            var keysToDelete = rules.Select(id => (RedisKey)GetKey(id.ToString())).ToList();
            keysToDelete.Add(POINT_RULE_INDEX_KEY); // Xóa luôn cái Index

            await _db.KeyDeleteAsync(keysToDelete.ToArray());
        }

        public async Task<List<PointRule>> GetOrSetAllAsync(Func<Task<List<PointRule>>> getAllFromDb)
        {
            // 1.Kiểm tra xem Index có tồn tại không
            var ruleIds = await _db.SetMembersAsync(POINT_RULE_INDEX_KEY);

            if (ruleIds.Length > 0)
            {
                // Cache Hit: Lấy từng item dựa trên ID trong Index
                var tasks = ruleIds.Select(id => GetPointRuleAsync(Guid.Parse(id!)));
                var results = await Task.WhenAll(tasks);
                return results.Where(r => r != null).ToList()!;
            }

            // 2. Cache Miss: Gọi Database thông qua Callback
            var ruleFromDb = await getAllFromDb();

            if (ruleFromDb != null && ruleFromDb.Any())
            {
                // 3. Nạp vào Redis để lần sau dùng (Lazy Load)
                foreach (var tier in ruleFromDb)
                {
                    await SetPointRuleAsync(tier.Id, tier);
                }
            }

            return ruleFromDb ?? new List<PointRule>();
        }

        public async Task<PointRule?> GetPointRuleAsync(Guid ruleId)
        {
            var data = await _db.StringGetAsync(GetKey(ruleId.ToString()));
            return data.HasValue ? JsonSerializer.Deserialize<PointRule>(data!) : default;
        }

        public async Task SetPointRuleAsync(Guid ruleId, PointRule data)
        {
            var key = GetKey(ruleId.ToString());
            var jsonData = JsonSerializer.Serialize(data);

            // Dùng Transaction để đảm bảo tính toàn vẹn giữa Data và Index
            var batch = _db.CreateTransaction();

            // Lưu dữ liệu (Không truyền tham số expiry = vĩnh viễn)
            _ = batch.StringSetAsync(key, jsonData);

            // Đánh dấu ID này vào Index
            _ = batch.SetAddAsync(POINT_RULE_INDEX_KEY, ruleId.ToString());

            await batch.ExecuteAsync();
        }
    }
}
