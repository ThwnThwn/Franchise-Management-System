using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FranchiseService.Application.Interfaces;
using FranchiseService.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FranchiseService.Infrastructure.Services;

/// <summary>
/// Implementation giao tiếp với DocuSeal REST API.
///
/// ─── LUỒNG HOẠT ĐỘNG CHI TIẾT ──────────────────────────────────────────────
///
/// [1] ADMIN TẠO TEMPLATE TRƯỚC
///     • Vào DocuSeal Dashboard → Templates → New Template
///     • Upload file PDF hợp đồng nhượng quyền (.pdf)
///     • Kéo thả các placeholder field: {{franchisee_name}}, {{store_address}},
///       {{franchise_fee}}, chữ ký (Signature Field), ngày ký (Date Field)...
///     • Lưu → DocuSeal cấp Template ID (ví dụ: 123)
///     • Lưu Template ID này vào config/appsettings của service
///
/// [2] KHI APPLICATION ĐƯỢC APPROVE → GỌI CreateSubmissionAsync
///     POST https://api.docuseal.com/submissions
///     Header: X-Auth-Token: {ApiToken}
///     Body: {
///       "template_id": 123,
///       "send_email": true,           ← DocuSeal tự gửi email có link ký
///       "submitters": [{
///         "role": "First Party",       ← tên role khớp với Template
///         "email": "partner@email.com",
///         "name": "Nguyễn Văn A",
///         "fields": [                  ← pre-fill data vào template
///           { "name": "franchisee_name", "default_value": "Nguyễn Văn A" },
///           { "name": "store_address",   "default_value": "123 Lê Lợi, Q1" },
///           { "name": "franchise_fee",   "default_value": "50,000,000 VNĐ" }
///         ]
///       }]
///     }
///     Response: { "id": 456, "status": "pending", "submitters": [{ "slug": "abc", "embed_src": "..." }] }
///     → Lưu id=456 vào FranchiseContract.DocusealSubmissionId
///     → ContractStatus = AwaitingSignature
///
/// [3] PARTNER KÝ TRÊN DOCUSEAL
///     • DocuSeal gửi email link ký cho partner (hoặc dùng embed_src nhúng vào web)
///     • Partner bấm link → điền thông tin → ký số → submit
///
/// [4] DOCUSEAL GỬI WEBHOOK VỀ SERVER KHI KÝ XONG
///     POST {your_server}/api/franchise/webhook/docuseal
///     Header: X-DocuSeal-Signature: {HMAC-SHA256(webhookSecret, rawBody)}
///     Body: {
///       "event_type": "form.completed",
///       "timestamp": "2026-03-18T...",
///       "data": {
///         "submission_id": 456,
///         "status": "completed",
///         "email": "partner@email.com",
///         "completed_at": "2026-03-18T...",
///         "document_url": "https://docuseal.com/blobs/..."   ← PDF đã ký
///       }
///     }
///
/// [5] SERVER XỬ LÝ WEBHOOK
///     a. Verify chữ ký HMAC để xác nhận request từ DocuSeal
///     b. Tìm FranchiseContract theo submission_id
///     c. Download PDF từ document_url → upload lên Cloudinary/S3
///     d. Update: ContractStatus = AwaitingPayment, ContractPdfUrl = new secure link
///     e. Gửi thông báo cho Kế toán / Giám đốc
///
/// ────────────────────────────────────────────────────────────────────────────
/// </summary>
public class DocuSealService : IDocuSealService
{
    private readonly HttpClient _httpClient;
    private readonly DocuSealSettings _settings;
    private readonly ILogger<DocuSealService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public DocuSealService(
        IHttpClientFactory httpClientFactory,
        IOptions<DocuSealSettings> settings,
        ILogger<DocuSealService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("DocuSeal");
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", _settings.ApiToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <inheritdoc/>
    public async Task<DocuSealCreateSubmissionResult> CreateSubmissionAsync(
        int templateId,
        string signerEmail,
        string signerName,
        Dictionary<string, string>? metadata,
        CancellationToken cancellationToken = default)
    {
        // Tạo danh sách pre-fill fields từ metadata (điền sẵn thông tin vào hợp đồng)
        var fields = metadata?
            .Select(kv => new { name = kv.Key, default_value = kv.Value })
            .ToList<object>() ?? [];

        var payload = new
        {
            template_id = templateId,
            send_email = true,  // DocuSeal tự gửi email link ký cho đối tác
            submitters = new[]
            {
                new
                {
                    role = "Bên B",  // Khớp với tên Role trong Template DocuSeal
                    email = signerEmail,
                    name = signerName,
                    fields
                }
            }
        };

        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("DocuSeal: Creating submission for template {TemplateId}, signer {Email}", templateId, signerEmail);

        var response = await _httpClient.PostAsync("/submissions", content, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("DocuSeal CreateSubmission failed. Status: {Status}, Body: {Body}", response.StatusCode, responseBody);
            throw new Exception($"DocuSeal API error {(int)response.StatusCode}: {responseBody}");
        }

        // DocuSeal trả về Array khi tạo với 1 submitter, ta lấy phần tử đầu tiên
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        // Response có thể là array hoặc object tùy version DocuSeal
        var submissionElement = root.ValueKind == JsonValueKind.Array
            ? root[0]
            : root;

        return new DocuSealCreateSubmissionResult
        {
            Id = submissionElement.TryGetProperty("submission_id", out var sid)
                ? sid.GetInt32()
                : submissionElement.GetProperty("id").GetInt32(),
            Status = submissionElement.TryGetProperty("status", out var status)
                ? status.GetString() ?? "pending"
                : "pending",
            SignUrl = submissionElement.TryGetProperty("embed_src", out var embedSrc)
                ? embedSrc.GetString()
                : null
        };
    }

    /// <inheritdoc/>
    public bool VerifyWebhookSignature(string rawBody, string signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
        {
            _logger.LogWarning("DocuSeal WebhookSecret is not configured — skipping signature verification.");
            return true; // không verify nếu chưa config (chỉ dùng khi dev)
        }

        try
        {
            // DocuSeal gửi header dạng: X-DocuSeal-Signature: sha256=<hmac_hex>
            var expectedHex = signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
                ? signatureHeader[7..]
                : signatureHeader;

            var keyBytes = Encoding.UTF8.GetBytes(_settings.WebhookSecret);
            var bodyBytes = Encoding.UTF8.GetBytes(rawBody);

            using var hmac = new HMACSHA256(keyBytes);
            var computedHash = hmac.ComputeHash(bodyBytes);
            var computedHex = Convert.ToHexString(computedHash).ToLowerInvariant();

            // So sánh constant-time để tránh timing attack
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHex),
                Encoding.UTF8.GetBytes(expectedHex.ToLowerInvariant()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DocuSeal: Error verifying webhook signature");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetSubmissionPdfUrlAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/submissions/{submissionId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("DocuSeal: Could not fetch submission {Id}, status {Status}", submissionId, response.StatusCode);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Thử lấy URL từ documents array
        if (root.TryGetProperty("documents", out var docs) && docs.ValueKind == JsonValueKind.Array)
        {
            foreach (var docItem in docs.EnumerateArray())
            {
                if (docItem.TryGetProperty("url", out var urlProp))
                    return urlProp.GetString();
            }
        }

        return null;
    }
}
