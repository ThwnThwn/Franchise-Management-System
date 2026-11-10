namespace FranchiseService.Application.Features.Shifts.DTOs;

/// <summary>
/// DTO chi tiết cho bản ghi chấm công, bao gồm thông tin nhân viên và ca làm.
/// Dùng cho API GetTimeLogs (Manager xem bảng công / Staff xem lịch sử).
/// </summary>
public class TimeLogDto
{
    public Guid Id { get; set; }
    public Guid ShiftAssignmentId { get; set; }
    public DateTimeOffset? CheckInAt { get; set; }
    public string? CheckInImageUrl { get; set; }
    public decimal? CheckInLat { get; set; }
    public decimal? CheckInLong { get; set; }
    public DateTimeOffset? CheckOutAt { get; set; }
    public string? CheckOutImageUrl { get; set; }
    public decimal? CheckOutLat { get; set; }
    public decimal? CheckOutLong { get; set; }

    /// <summary>
    /// Số phút làm việc bình thường trong ca (tối đa bằng ngưỡng quy định trong ngày).
    /// </summary>
    public int RegularMinutes { get; set; }

    /// <summary>
    /// Số phút làm thêm giờ (Overtime) vượt ngưỡng trong ngày.
    /// </summary>
    public int DailyOtMinutes { get; set; }

    /// <summary>
    /// Số phút làm thêm giờ (Overtime) vượt ngưỡng trong tuần.
    /// </summary>
    public int WeeklyOtMinutes { get; set; }

    public int? Status { get; set; }
    public string? ManagerNote { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Thông tin lồng nhau - FE cần hiển thị tên nhân viên và thời gian ca gốc
    public TimeLogStaffInfoDto Staff { get; set; } = new();
    public TimeLogShiftInfoDto Shift { get; set; } = new();
}

public class TimeLogStaffInfoDto
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Email { get; set; }
}

public class TimeLogShiftInfoDto
{
    public Guid Id { get; set; }
    public DateOnly ShiftDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Note { get; set; }
}
