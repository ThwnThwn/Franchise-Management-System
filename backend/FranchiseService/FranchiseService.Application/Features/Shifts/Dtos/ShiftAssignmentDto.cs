namespace FranchiseService.Application.Features.Shifts.DTOs
{
    /// <summary>
    /// DTO trả về thông tin phân công nhân viên trong ca làm việc.
    /// </summary>
    public class ShiftAssignmentDto
    {
        /// <summary>
        /// ID phân công
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID ca làm việc
        /// </summary>
        public Guid ShiftId { get; set; }

        /// <summary>
        /// ID nhân viên (Identity User)
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Trạng thái phân công: 0:Assigned, 1:Confirmed, 2:Rejected
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Mức lương theo giờ tại thời điểm phân công (Snapshot)
        /// </summary>
        public decimal? HourlyWage { get; set; }

        /// <summary>
        /// Tên nhân viên (lấy từ Identity Service qua gRPC)
        /// </summary>
        public string? StaffName { get; set; }

        /// <summary>
        /// Avatar URL (lấy từ Identity Service)
        /// </summary>
        public string? StaffAvatarUrl { get; set; }

        /// <summary>
        /// Email nhân viên (lấy từ Identity Service)
        /// </summary>
        public string? StaffEmail { get; set; }

        public List<TimeLogDto> TimeLogs { get; set; } = new();
    }
}
