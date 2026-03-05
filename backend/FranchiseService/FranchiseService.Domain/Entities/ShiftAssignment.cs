using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities
{
    /// <summary>
    /// Đại diện cho việc Phân công nhân viên vào một Ca làm việc.
    /// Bảng: shift_assignments
    /// </summary>
    public class ShiftAssignment : SoftDeletedEntity, IEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// ID Ca làm việc
        /// </summary>
        public Guid ShiftId { get; set; }

        /// <summary>
        /// ID Nhân viên được gán (Identity User)
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

        public bool IsOverridden { get; set; }
        public string? OverrideReason { get; set; }
        public Guid? OverriddenBy { get; set; }


        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // --- Navigation Properties ---
        public virtual Shift Shift { get; set; } = null!;
        
        // TimeLog link trực tiếp vào Assignment
        public virtual ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
    }
}