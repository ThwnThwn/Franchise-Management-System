using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Persistences.Caches;
using IdentityService.Application.AppConfigurations;
using IdentityService.Application.Persistences.Caches;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistences.Caches;

public class OtpStore : IOtpStore
{
    private readonly IDistributedCache _cache;
    private readonly OtpConfigs _otpCfg;

    public OtpStore(IDistributedCache cache, IOptions<OtpConfigs> otpCfg)
    {
        _cache = cache;
        _otpCfg = otpCfg.Value;
    }

    public async Task SaveOTPAsyns(string key, string otp)
    {
        await _cache.SetStringAsync(
                $"otp:{key}",
                otp,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_otpCfg.OtpTtl)
                }
            );
    }

    public async Task<string?> GetOtpAsync(string key)
    {
        return await _cache.GetStringAsync($"otp:{key}");
    }

    public async Task RemoveOTPAsync(string key)
    {
        await _cache.RemoveAsync($"otp:{key}");
    }

    public async Task SaveRegistrationDataAsync(string key, string data)
    {
        await _cache.SetStringAsync(
                $"register:{key}",
                data,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_otpCfg.OtpTtl)
                }
            );
    }

    public async Task<string?> GetRegistrationDataAsync(string key)
    {
        return await _cache.GetStringAsync($"register:{key}");
    }

    public async Task RemoveRegistrationDataAsync(string key)
    {
        await _cache.RemoveAsync($"register:{key}");
    }
}