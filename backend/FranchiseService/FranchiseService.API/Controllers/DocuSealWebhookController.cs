using FranchiseService.Application.Features.FranchiseContracts.Commands.HandleDocuSealWebhook;
using FranchiseService.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace FranchiseService.API.Controllers;

/// <summary>
/// Controller đón tiếp "tín hiệu" từ DocuSeal Webhook.
/// </summary>
[Route("api/franchise/webhook/docuseal")]
[ApiController]
public class DocuSealWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDocuSealService _docuSeal;
    private readonly ILogger<DocuSealWebhookController> _logger;

    public DocuSealWebhookController(
        IMediator mediator,
        IDocuSealService docuSeal,
        ILogger<DocuSealWebhookController> logger)
    {
        _mediator = mediator;
        _docuSeal = docuSeal;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "DocuSeal Webhook endpoint is alive!", time = DateTime.Now });
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] DocuSealWebhookJson payload)
    {
        // 1. Log chi tiết để dễ debug
        var eventType = payload.EventType ?? payload.Event ?? string.Empty;
        var submissionId = payload.Data?.SubmissionId ?? payload.Data?.Id ?? 0;
        var documentUrl = payload.Data?.Documents?.FirstOrDefault()?.Url;

        _logger.LogInformation(
            "DocuSeal Webhook received: EventType={EventType}, Event={Event}, " +
            "ResolvedEvent={Resolved}, SubmissionId={Id}, DocumentUrl={Url}", 
            payload.EventType, payload.Event, eventType, submissionId, documentUrl);

        // 2. Bảo mật (Tùy chọn): Verify chữ ký từ Header để chắc chắn là DocuSeal gọi
        var signature = Request.Headers["X-DocuSeal-Signature"].ToString();

        // 3. Chuyển đổi dữ liệu sang Command
        var command = new HandleDocuSealWebhookCommand
        {
            EventType = eventType,
            SubmissionId = submissionId,
            DocumentUrl = documentUrl,
            CompletedAt = payload.Data?.CompletedAt
        };

        // 4. Gửi vào hệ thống để xử lý (Tự động tải file + Đổi status)
        var result = await _mediator.Send(command);

        return result ? Ok(new { message = "Webhook processed successfully" }) 
                      : BadRequest(new { message = "Webhook skipped or failed" });
    }
}

// Model khớp 100% với JSON của DocuSeal
public class DocuSealWebhookJson
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }

    [JsonPropertyName("data")]
    public DocuSealDataJson? Data { get; set; }
}

public class DocuSealDataJson
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("submission_id")]
    public int? SubmissionId { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("documents")]
    public List<DocuSealDocumentJson>? Documents { get; set; }
}

public class DocuSealDocumentJson
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
