using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

/// <summary>
/// Yeu cau doi ca giua 2 nhan vien.
/// Bang: shift_swap_requests
/// </summary>
public class ShiftSwapRequest : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// ID phan cong ca cua nguoi gui (A)
    /// </summary>
    public Guid SourceAssignmentId { get; set; }

    /// <summary>
    /// ID nguoi gui yeu cau (A)
    /// </summary>
    public Guid SourceUserId { get; set; }

    /// <summary>
    /// ID nguoi duoc yeu cau nhan ca (B)
    /// </summary>
    public Guid TargetUserId { get; set; }

    /// <summary>
    /// Trang thai: 0=PendingTarget, 1=PendingManager, 2=Approved,
    /// 3=Rejected, 4=Expired, 5=Cancelled
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Ly do gui yeu cau (bat buoc)
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Ly do tu choi (B hoac Manager dien khi reject)
    /// </summary>
    public string? RejectReason { get; set; }

    /// <summary>
    /// Thoi han B phai tra loi (qua han thi Expired)
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// ID Manager da duyet hoac tu choi
    /// </summary>
    public Guid? ReviewedBy { get; set; }

    /// <summary>
    /// Thoi diem Manager duyet
    /// </summary>
    public DateTimeOffset? ReviewedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public virtual ShiftAssignment SourceAssignment { get; set; } = null!;
}
