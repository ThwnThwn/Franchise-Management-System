using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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
    /// VNPay signs using WebUtility.UrlEncode (space → +), sorted by key.
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

    public static async Task<Dictionary<string, string>> CallRefundApiAsync(
        string refundUrl,
        string hashSecret,
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        // ===== STEP 1: SORT + SIGN =====
        var sorted = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);

        // VNPay Refund API ký trên raw query (không URL encode)
        var rawQuery = string.Join("|", sorted.Select(kv => kv.Value));
        var secureHash = HmacSHA512(hashSecret, rawQuery);

        // ===== STEP 2: BUILD REQUEST BODY =====
        var requestBody = new Dictionary<string, string>(sorted)
        {
            ["vnp_SecureHash"] = secureHash
        };

        var jsonBody = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        // ===== STEP 3: POST TO VNPAY =====
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await httpClient.PostAsync(refundUrl, httpContent, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return new Dictionary<string, string>
            {
                ["vnp_ResponseCode"] = "99",
                ["vnp_Message"] = "Request timeout"
            };
        }
        catch (HttpRequestException ex)
        {
            return new Dictionary<string, string>
            {
                ["vnp_ResponseCode"] = "99",
                ["vnp_Message"] = $"HTTP error: {ex.Message}"
            };
        }

        // ===== STEP 4: PARSE RESPONSE =====
        var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var responseDict = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
            return responseDict ?? new Dictionary<string, string>
            {
                ["vnp_ResponseCode"] = "99",
                ["vnp_Message"] = "Empty response from VNPay"
            };
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>
            {
                ["vnp_ResponseCode"] = "99",
                ["vnp_Message"] = $"Invalid JSON response: {responseContent}"
            };
        }
    }

    public static bool ValidateSignature(Dictionary<string, string> query, string hashSecret)
    {
        if (!query.TryGetValue("vnp_SecureHash", out var receivedHash))
            return false;

        var paramsToSign = query
            .Where(x => x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType")
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}={x.Value}");

        var rawData = string.Join("&", paramsToSign);
        var computedHash = HmacSHA512(hashSecret, rawData);

        return computedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
    }

}

