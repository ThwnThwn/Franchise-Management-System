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

    public async Task<DateTimeOffset?> GetLastOtpRequestTimeAsync(string key)
    {
        var value = await _cache.GetStringAsync($"otp_request_time:{key}");
        if (string.IsNullOrEmpty(value))
            return null;

        if (DateTimeOffset.TryParse(value, out var time))
            return time;

        return null;
    }

    public async Task SaveLastOtpRequestTimeAsync(string key, DateTimeOffset time)
    {
        await _cache.SetStringAsync(
            $"otp_request_time:{key}",
            time.ToString("O"),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            }
        );
    }

    public async Task<int> GetOtpAttemptCountAsync(string key)
    {
        var value = await _cache.GetStringAsync($"otp_attempts:{key}");
        if (string.IsNullOrEmpty(value))
            return 0;

        if (int.TryParse(value, out var count))
            return count;

        return 0;
    }

    public async Task IncrementOtpAttemptAsync(string key)
    {
        var currentCount = await GetOtpAttemptCountAsync(key);
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

    public async Task ResetOtpAttemptAsync(string key)
    {
        await _cache.RemoveAsync($"otp_attempts:{key}");
    }

    public async Task RemoveForgetPasswordDataAsync(string key)
    {
        await _cache.RemoveAsync($"forgetpassword:{key}");
    }
}