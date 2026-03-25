using System.Text.Json.Serialization;

namespace FranchiseService.Application.Interfaces;

/// <summary>
/// Interface giao tiếp với DocuSeal API.
/// DocuSeal là dịch vụ ký văn bản điện tử (e-signature) tương tự DocuSign.
/// Flow chính:
///   1. Admin tạo Template trên DocuSeal Dashboard (PDF hợp đồng với placeholders)
///   2. Khi Application được Approve → gọi CreateSubmissionAsync để tạo yêu cầu ký mới
///   3. DocuSeal gửi email cho đối tác kèm link ký → đối tác ký online
///   4. Khi ký xong → DocuSeal POST Webhook event "form.completed" về server
///   5. Server nhận Webhook → cập nhật ContractStatus = AwaitingPayment + lưu ContractPdfUrl
/// </summary>
public interface IDocuSealService
{
    /// <summary>
    /// Tạo một Submission mới trên DocuSeal để gửi hợp đồng cho đối tác ký.
    /// </summary>
    /// <param name="templateId">ID của Template hợp đồng đã tạo sẵn trên DocuSeal Dashboard.</param>
    /// <param name="signerEmail">Email của người ký (đối tác nhượng quyền).</param>
    /// <param name="signerName">Họ tên người ký.</param>
    /// <param name="metadata">Dữ liệu tùy chỉnh điền vào template (tên công ty, địa chỉ, phí...).</param>
    /// <returns>ID của Submission vừa tạo — lưu vào FranchiseContract.DocusealSubmissionId.</returns>
    Task<DocuSealCreateSubmissionResult> CreateSubmissionAsync(
        int templateId,
        string signerEmail,
        string signerName,
        Dictionary<string, string>? metadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xác thực chữ ký HMAC-SHA256 của Webhook từ DocuSeal.
    /// Cần verify để đảm bảo request đến thực sự từ DocuSeal, không phải từ bên thứ 3 giả mạo.
    /// DocuSeal gửi header: X-DocuSeal-Signature = HMAC-SHA256(webhookSecret, rawBody)
    /// </summary>
    bool VerifyWebhookSignature(string rawBody, string signatureHeader);

    /// <summary>
    /// Lấy link download PDF hợp đồng đã ký từ DocuSeal (dùng sau khi form.completed).
    /// </summary>
    Task<string?> GetSubmissionPdfUrlAsync(int submissionId, CancellationToken cancellationToken = default);
}

public class DocuSealCreateSubmissionResult
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// Link ký cho submitter đầu tiên — có thể gửi email manual hoặc DocuSeal tự gửi.
    /// </summary>
    public string? SignUrl { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────────
// Webhook Payload Models
// DocuSeal POST về server khi có sự kiện. Các event quan trọng:
//   • form.viewed    — Người ký đã mở link
//   • form.started   — Người ký bắt đầu điền
//   • form.completed — Người ký đã hoàn thành (QUAN TRỌNG NHẤT)
// Header: X-DocuSeal-Signature = HMAC-SHA256(secret, body)
// ──────────────────────────────────────────────────────────────────────────────

public class DocuSealWebhookPayload
{
    /// <summary>Event type: "form.viewed" | "form.started" | "form.completed"</summary>
    [JsonPropertyName("event_type")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("data")]
    public DocuSealWebhookData Data { get; set; } = new();
}

public class DocuSealWebhookData
{
    /// <summary>
    /// ID của Submission (là DocusealSubmissionId ta lưu trong FranchiseContract).
    /// </summary>
    [JsonPropertyName("submission_id")]
    public int SubmissionId { get; set; }

    /// <summary>Trạng thái submission: "pending" | "completed" | "expired"</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>ID of the form/submitter step (mỗi người ký = 1 form)</summary>
    [JsonPropertyName("id")]
    public int FormId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// URL download PDF đã ký — có thể null khi form chỉ "started".
    /// Khi "form.completed", DocuSeal trả về link này.
    /// Ta nên download rồi re-upload lên S3/Cloudinary để tránh bị expire.
    /// </summary>
    [JsonPropertyName("document_url")]
    public string? DocumentUrl { get; set; }

    /// <summary>Danh sách các field đã được điền/ký trong form.</summary>
    [JsonPropertyName("values")]
    public List<DocuSealFieldValue>? Values { get; set; }
}

public class DocuSealFieldValue
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
