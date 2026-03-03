using SalesService.Domain.Enums;

namespace SalesService.Application.Common.Models;

/// <summary>
/// Thông tin override dành cho nhân sự có quyền cao (manager / InternalUser)
/// Có audit & kiểm soát rule
/// </summary>
public sealed class InternalUserOverrideInfo
{
    public Guid InternalUserId { get; set; }

    public bool AutoApprove { get; set; } = false;

    // Bỏ qua giới hạn thời gian refund
    public bool BypassTimeLimit { get; set; } = false;

    // Cho phép refund vượt quá transaction gốc (bồi thường)
    public bool BypassAmountValidation { get; set; } = false;

    public RefundReasonType? RefundReason { get; set; }

    public string? InternalNotes { get; set; }
}
