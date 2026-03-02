using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

/// <summary>
/// Don dang ky nhan ca tu Cho Ca (Shift Pool).
/// Nhan vien thay ca dang Open tren App, bam "Dang ky nhan ca".
/// Manager vao Web xem danh sach don roi Duyet (Approve) hoac Tu choi (Reject).
/// Bang: shift_applications
/// </summary>
public class ShiftApplication : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// Ca lam viec ma nhan vien muon dang ky nhan
    /// </summary>
    public Guid ShiftId { get; set; }

    /// <summary>
    /// ID nhan vien gui don dang ky (tu Identity Service)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Trang thai don: 0 = Pending, 1 = Approved, 2 = Rejected
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Ghi chu cua nhan vien khi dang ky (vi du: "Em ranh ca nay, xin nhan")
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Thoi diem nhan vien bam nut "Dang ky nhan ca"
    /// </summary>
    public DateTimeOffset AppliedAt { get; set; }

    /// <summary>
    /// Thoi diem Manager duyet hoac tu choi don
    /// </summary>
    public DateTimeOffset? ReviewedAt { get; set; }

    /// <summary>
    /// ID cua Manager da duyet hoac tu choi don nay
    /// </summary>
    public Guid? ReviewedBy { get; set; }

    /// <summary>
    /// Ly do Manager duyet hoac tu choi (vi du: "Tu choi vi da du nguoi")
    /// </summary>
    public string? ReviewNote { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // --- Navigation Properties ---
    public virtual Shift Shift { get; set; } = null!;
}
