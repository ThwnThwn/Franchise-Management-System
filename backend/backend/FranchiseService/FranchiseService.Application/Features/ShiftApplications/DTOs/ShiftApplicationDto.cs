namespace FranchiseService.Application.Features.ShiftApplications.DTOs;

/// <summary>
/// DTO tra ve thong tin don dang ky kem diem Smart Score.
/// Dung cho Manager xem danh sach ung vien cua 1 ca.
/// </summary>
public class ShiftApplicationDto
{
    public Guid Id { get; set; }
    public Guid ShiftId { get; set; }
    public Guid UserId { get; set; }
    public int Status { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset AppliedAt { get; set; }

    /// <summary>
    /// Ho ten nhan vien
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Email nhan vien
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// Anh dai dien
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Muc luong theo gio hien tai (tu Identity Service)
    /// </summary>
    public decimal? HourlyWage { get; set; }

    /// <summary>
    /// Diem Smart Score do he thong tinh tu dong.
    /// Cong thuc: 0.5 * CostScore + 0.5 * FairnessScore
    ///   - CostScore: luong cang thap diem cang cao (tiet kiem cho quan)
    ///   - FairnessScore: gio tuan cang it diem cang cao (cong bang)
    /// Score cao = ung vien tot. Thang diem 0.0 - 1.0
    /// </summary>
    public double SmartScore { get; set; }

    /// <summary>
    /// Tong so gio da lam trong tuan (de Manager tham khao)
    /// </summary>
    public double WeeklyHoursWorked { get; set; }
}
