using System.Security.Cryptography;
using System.Text;
using System.Net;

namespace SupplierService.Application.Helpers;

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
        var validParams = parameters.Where(x => !string.IsNullOrEmpty(x.Value))
                                    .ToDictionary(x => x.Key, x => x.Value);

        var sorted = new SortedDictionary<string, string>(validParams, StringComparer.Ordinal);
        var query = string.Join("&", sorted.Select(kv =>
            $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

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
            sb.Append(b.ToString("x2"));

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
        // 1. Chỉ lấy những param có value
        var validParams = parameters.Where(x => !string.IsNullOrEmpty(x.Value))
                                    .ToDictionary(x => x.Key, x => x.Value);

        // 2. Sắp xếp theo alphabet (Ordinal)
        var sorted = new SortedDictionary<string, string>(validParams, StringComparer.Ordinal);

        // 3. Nối chuỗi & Encode cả Key lẫn Value bằng UrlEncode giống SDK VNPay
        var query = string.Join("&", sorted.Select(kv =>
            $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

        var calculated = HmacSHA512(hashSecret, query);
        return string.Equals(calculated, secureHash, StringComparison.OrdinalIgnoreCase);
    }
}
