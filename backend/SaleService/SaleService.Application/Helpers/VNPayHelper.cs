using System.Security.Cryptography;
using System.Text;
using System.Net;

namespace SalesService.Application.Helpers;

public static class VNPayHelper
{
    /// <summary>
    /// Build VNPay payment URL from base URL, secret and parameters.
    /// </summary>
    public static string BuildPaymentUrl(
        string baseUrl,
        string hashSecret,
        IDictionary<string, string> parameters)
    {
        // Sort parameters by key (VNPay requires lexicographical order)
        var sorted = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);

        // Build query string with URL encoding
        var query = string.Join("&", sorted.Select(kv =>
            $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));

        // Compute secure hash
        var secureHash = HmacSHA512(hashSecret, query);

        return $"{baseUrl}?{query}&vnp_SecureHashType=HmacSHA512&vnp_SecureHash={secureHash}";
    }

    /// <summary>
    /// Compute HMAC SHA512 hash (lowercase hex string).
    /// </summary>
    public static string HmacSHA512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);

        using var hmac = new HMACSHA512(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);

        var sb = new StringBuilder(hashValue.Length * 2);
        foreach (var b in hashValue)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Verify VNPay signature with given parameters and secure hash.
    /// </summary>
    public static bool VerifySignature(
        string hashSecret,
        IDictionary<string, string> parameters,
        string secureHash)
    {
        var sorted = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
        var query = string.Join("&", sorted.Select(kv =>
            $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));

        var calculated = HmacSHA512(hashSecret, query);
        return string.Equals(calculated, secureHash, StringComparison.OrdinalIgnoreCase);
    }
}

