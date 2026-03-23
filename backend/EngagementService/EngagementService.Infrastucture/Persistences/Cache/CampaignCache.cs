using IdentityService.Application.Abstrations;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Infrastucture.Persistences.Cache
{
    internal class CampaignCache : ICampaignCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly int _dbIndex;

        // Create script ONCE for maximum performance (EVALSHA)
        // -1 co voucher roi
        // 0 - khong du quantity
        // 1 - oke
        private static readonly LuaScript _redeemScript = LuaScript.Prepare(
            @"local stock = tonumber(redis.call('GET', @stockKey))
            if not stock or stock <= 0 then return 0 end
    
            -- Check if user already has it
            if redis.call('SISMEMBER', @usersKey, @userId) == 1 then return -1 end
    
            -- Decrement and Add user
            redis.call('DECR', @stockKey)
            redis.call('SADD', @usersKey, @userId)
            return 1"
        );

        public CampaignCache(IConnectionMultiplexer redis, int dbIndex)
        {
            _redis = redis;
            _dbIndex = dbIndex;
        }
        // Up Campaign len redis
        public async Task WarmUpVoucherAsync(Guid voucherId, int quanity, DateTimeOffset endDate)
        {
            var db = _redis.GetDatabase(_dbIndex);
            var stockKey = (RedisKey)$"v_stock:{voucherId}";
            var usersKey = (RedisKey)$"v_users:{voucherId}";

            // set the stock
            await db.StringSetAsync(stockKey, quanity);

            // clear previous participants (in case of a restart/testing)
            await db.KeyDeleteAsync(usersKey);

            // set TTL - tự xóa
            var ttl = (endDate - DateTimeOffset.UtcNow).Add(TimeSpan.FromHours(1));
            if (ttl.Ticks > 0)
            {
                await db.KeyExpireAsync(stockKey, ttl);
                await db.KeyExpireAsync(usersKey, ttl);
            }
        }

        public async Task<int> RedeemVoucherAsync(Guid voucherId, Guid userId)
        {
            var db = _redis.GetDatabase(_dbIndex);

            // This method automatically maps @stockKey, @usersKey, and @userId
            var result = await _redeemScript.EvaluateAsync(db, new
            {
                stockKey = (RedisKey)$"v_stock:{voucherId}",
                usersKey = (RedisKey)$"v_users:{voucherId}",
                userId = (RedisValue)userId.ToString()
            });

            return (int)result;
        }

        public async Task DeleteVoucherCacheAsync(Guid voucherId)
        {
            var db = _redis.GetDatabase(_dbIndex);
            // Remove both the Voucherstock and the user list
            await db.KeyDeleteAsync(new RedisKey[] { $"v_stock:{voucherId}", $"v_users:{voucherId}" });
        }
    }
}