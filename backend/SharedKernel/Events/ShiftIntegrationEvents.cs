namespace SharedKernel.Events;

/// <summary>
/// Sự kiện khi tạo ca làm việc mới.
/// Consumer cần biết đầy đủ thông tin giờ giấc để hiển thị hoặc xử lý.
/// </summary>
public class ShiftCreatedEvent
{
    public Guid ShiftId { get; set; }
    public Guid StoreId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int RequiredStaffCount { get; set; }
    public int BreakDurationMinutes { get; set; }
    public string Action { get; set; } = "ShiftCreated";
}

/// <summary>
/// Sự kiện khi cập nhật ca làm việc.
/// Consumer cần thông tin giờ giấc mới để cập nhật UI/Notification.
/// </summary>
public class ShiftUpdatedEvent
{
    public Guid ShiftId { get; set; }
    public Guid StoreId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int RequiredStaffCount { get; set; }
    public int BreakDurationMinutes { get; set; }
    public string Action { get; set; } = "ShiftUpdated";
}

/// <summary>
/// Sự kiện khi xóa ca làm việc (Soft Delete).
/// Consumer cần StoreId và ShiftDate để xử lý context.
/// </summary>
public class ShiftDeletedEvent
{
    public Guid ShiftId { get; set; }
    public Guid StoreId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public string Action { get; set; } = "ShiftDeleted";
}

/// <summary>
/// Sự kiện khi gán nhân viên vào ca.
/// Consumer cần HourlyWage và StoreId cho Reporting Service.
/// </summary>
public class ShiftAssignedEvent
{
    public Guid AssignmentId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid UserId { get; set; }
    public Guid StoreId { get; set; }
    public decimal? HourlyWage { get; set; }
    public string Action { get; set; } = "ShiftAssigned";
}
