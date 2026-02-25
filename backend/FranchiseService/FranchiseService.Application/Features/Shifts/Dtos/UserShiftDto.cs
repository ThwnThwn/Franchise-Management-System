namespace FranchiseService.Application.Features.Shifts.Dtos
{
    public class UserShiftDto
    {
        public bool HasShiftToday { get; set; }
        public Guid? AssignmentId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public double? StoreLat { get; set; }
        public double? StoreLong { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;

        public bool CanCheckIn { get; set; }
        public bool CanCheckOut { get; set; }
        public bool IsOvertime { get; set; }

        public Guid? CurrentLogId { get; set; }

        // Các trường DateTime trả về ISO String: "2026-02-12T21:00:00Z"
        public string? CheckInAt { get; set; }

        public string? CheckOutAt { get; set; }
        public string ServerTime { get; set; } = string.Empty;
    }
}