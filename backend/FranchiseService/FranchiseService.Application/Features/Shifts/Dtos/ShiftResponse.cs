namespace FranchiseService.Application.Features.Shifts.DTOs
{
    /// <summary>
    /// DTO trả về thông tin ca làm việc cho API response.
    /// Khớp với FE ShiftResponse interface.
    /// </summary>
    public class ShiftResponse
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public DateOnly ShiftDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int RequiredStaffCount { get; set; }
        public int BreakDurationMinutes { get; set; }

        public Guid? ShiftTemplateId { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }

        /// <summary>
        /// Số nhân viên đã được gán vào ca này
        /// </summary>
        public int AssignedStaffCount { get; set; }

        /// <summary>
        /// Danh sách phân công nhân viên
        /// </summary>
        public List<ShiftAssignmentDto> Assignments { get; set; } = new();
    }
}
