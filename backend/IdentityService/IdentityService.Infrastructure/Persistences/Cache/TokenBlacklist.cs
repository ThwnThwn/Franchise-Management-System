using IdentityService.Application.Persistences.Caches;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure.Persistences.Cache
{
    public class TokenBlacklist : ITokenBlacklist
    {
        private const string BLACKLIST_KEY_PREFIX = "blacklist:token:";
        private readonly IDatabase _redisDb;

        public TokenBlacklist(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        public async Task AddToBlacklistAsync(string accessToken, DateTimeOffset expiryTime)
        {
            // Tạo key với prefix
            var key = $"{BLACKLIST_KEY_PREFIX}{accessToken}";

            // Tính thời gian sống còn lại (TTL)
            var timeToLive = expiryTime - DateTimeOffset.UtcNow;

            // Nếu token đã hết hạn, không cần blacklist
            if (timeToLive <= TimeSpan.Zero)
                return;

            // Lưu vào Redis với TTL tự động xóa
            await _redisDb.StringSetAsync(
                key,
                "revoked",
                timeToLive,
                When.Always
            );
        }

        public async Task<bool> IsTokenBlacklistedAsync(string accessToken)
        {
            var key = $"{BLACKLIST_KEY_PREFIX}{accessToken}";

            var exists = await _redisDb.KeyExistsAsync(key);
            return exists;
        }

        public async Task RemoveFromBlacklistAsync(string accessToken)
        {
            var key = $"{BLACKLIST_KEY_PREFIX}{accessToken}";
            await _redisDb.KeyDeleteAsync(key);
        }
    }
}
