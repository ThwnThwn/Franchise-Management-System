using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityService.Application.AppConfigurations;
using IdentityService.Application.Persistences.Caches;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace IdentityService.Infrastructure.Persistences.Cache;

public class OtpStore : IOtpStore
{
    private readonly IDistributedCache _cache;
    private readonly OtpConfigs _otpCfg;

    public OtpStore(IDistributedCache cache, IOptions<OtpConfigs> otpCfg)
    {
        _cache = cache;
        _otpCfg = otpCfg.Value;
    }

    public async Task SaveOTPAsyns(string key, string otp, string purpose)
    {
        await _cache.SetStringAsync(
                $"otp:{purpose}:{key}",
                otp,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_otpCfg.OtpTtl)
                }
            );
    }

    public async Task<string?> GetOtpAsync(string key, string purpose)
    {
        return await _cache.GetStringAsync($"otp:{purpose}:{key}");
    }

    public async Task RemoveOTPAsync(string key, string purpose)
    {
        await _cache.RemoveAsync($"otp:{purpose}:{key}");
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

    public async Task SaveForgetPasswordDataAsync(string key, string data)
    {
        await _cache.SetStringAsync(
                $"forgetpassword:{key}",
                data,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_otpCfg.OtpTtl)
                }
            );
    }

    public async Task<string?> GetForgetPasswordAsync(string key)
    {
        return await _cache.GetStringAsync($"forgetpassword:{key}");
    }

    public async Task<string?> GetRegistrationDataAsync(string key)
    {
        return await _cache.GetStringAsync($"register:{key}");
    }

    public async Task RemoveRegistrationDataAsync(string key)
    {
        await _cache.RemoveAsync($"register:{key}");
    }

    public async Task<DateTimeOffset?> GetLastOtpRequestTimeAsync(string key, string purpose)
    {
        var value = await _cache.GetStringAsync($"otp_request_time:{purpose}:{key}");
        if (string.IsNullOrEmpty(value))
            return null;

        if (DateTimeOffset.TryParse(value, out var time))
            return time;

        return null;
    }

    public async Task SaveLastOtpRequestTimeAsync(string key, DateTimeOffset time, string purpose)
    {
        await _cache.SetStringAsync(
            $"otp_request_time:{purpose}:{key}",
            time.ToString("O"),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            }
        );
    }

    public async Task<int> GetOtpAttemptCountAsync(string key, string purpose)
    {
        var value = await _cache.GetStringAsync($"otp_attempts:{purpose}:{key}");
        if (string.IsNullOrEmpty(value))
            return 0;

        if (int.TryParse(value, out var count))
            return count;

        return 0;
    }

    public async Task IncrementOtpAttemptAsync(string key, string purpose)
    {
        var currentCount = await GetOtpAttemptCountAsync(key, purpose);
        var newCount = currentCount + 1;

        await _cache.SetStringAsync(
            $"otp_attempts:{key}",
            newCount.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_otpCfg.OtpTtl)
            }
        );
    }

    public async Task ResetOtpAttemptAsync(string key, string purpose)
    {
        await _cache.RemoveAsync($"otp_attempts:{purpose}:{key}");
    }

    public async Task RemoveForgetPasswordDataAsync(string key)
    {
        await _cache.RemoveAsync($"forgetpassword:{key}");
    }
}