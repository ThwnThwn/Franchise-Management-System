using NotificationService.Domain.Enums;
namespace NotificationService.Domain.Entities;
public class NotificationLog
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ReferenceId { get; set; } // Dùng để Idempotency (VD: OrderId)
    public DateTime CreatedAt { get; set; }
}