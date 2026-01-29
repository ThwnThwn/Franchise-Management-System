namespace SharedKernel.Events;
/// <summary>
/// Event phát ra khi user yêu cầu reset password
/// Publisher: IdentityService
/// Consumer: NotificationService
/// </summary>
public class PasswordResetRequestedEvent
{
    /// <summary>
    /// Email người yêu cầu reset
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Token reset password (hoặc link reset)
    /// VD: "https://app.com/reset?token=abc123xyz"
    /// </summary>
    public string ResetToken { get; set; } = string.Empty;
    /// <summary>
    /// Thời điểm yêu cầu
    /// </summary>
    public DateTimeOffset RequestedAt { get; set; }
    /// <summary>
    /// Thời gian hết hạn của token (optional)
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}