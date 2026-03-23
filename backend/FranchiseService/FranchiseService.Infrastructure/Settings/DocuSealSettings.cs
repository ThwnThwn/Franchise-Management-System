namespace FranchiseService.Infrastructure.Settings;

public class DocuSealSettings
{
    public string BaseUrl { get; set; } = "https://api.docuseal.com";
    public string ApiToken { get; set; } = null!;
    /// <summary>
    /// Secret dùng để verify chữ ký HMAC-SHA256 trong Webhook header X-DocuSeal-Signature.
    /// </summary>
    public string WebhookSecret { get; set; } = null!;
    public int TemplateId { get; set; }
}
