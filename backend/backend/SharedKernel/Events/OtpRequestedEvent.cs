namespace SharedKernel.Events;
/// <summary>
/// Event phát ra khi có yêu cầu gửi OTP
/// Publisher: IdentityService (khi user Register hoặc Request OTP)
/// Consumer: NotificationService
/// </summary>
public class OtpRequestedEvent
{
    /// <summary>
    /// Email người nhận OTP
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Mã OTP cần gửi (6 chữ số)
    /// </summary>
    public string OtpCode { get; set; } = string.Empty;
    /// <summary>
    /// Thời điểm tạo OTP (để NotificationService log)
    /// </summary>
    public DateTimeOffset RequestedAt { get; set; }
}