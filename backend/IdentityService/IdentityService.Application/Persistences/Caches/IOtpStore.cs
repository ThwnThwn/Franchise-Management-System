using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistences.Caches;

public interface IOtpStore
{
    Task<string?> GetOtpAsync(string key);

    Task SaveOTPAsyns(string key, string otp);

    Task RemoveOTPAsync(string key);

    Task SaveRegistrationDataAsync(string key, string data);

    Task<string?> GetRegistrationDataAsync(string key);

    Task RemoveRegistrationDataAsync(string key);
}