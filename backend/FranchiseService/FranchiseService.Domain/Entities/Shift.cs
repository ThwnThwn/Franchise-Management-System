using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities
{
    /// <summary>
    /// Đại diện cho Cấu hình Ca làm việc (Nhu cầu nhân sự).
    /// Bảng: shifts
    /// </summary>
    public class Shift : SoftDeletedEntity, IEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// ID Cửa hàng
        /// </summary>
        public Guid StoreId { get; set; }

        /// <summary>
        /// Ngày làm việc
        /// </summary>
        public DateOnly ShiftDate { get; set; }

        /// <summary>
        /// Thời gian bắt đầu
        /// </summary>
        public TimeOnly StartTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc (Có thể nhỏ hơn StartTime nếu qua đêm)
        /// </summary>
        public TimeOnly EndTime { get; set; }

        /// <summary>
        /// Số lượng nhân viên cần thiết cho ca này
        /// </summary>
        public int RequiredStaffCount { get; set; }

        /// <summary>
        /// ID Mẫu ca (nếu được tạo từ mẫu)
        /// </summary>
        public Guid? ShiftTemplateId { get; set; }

        /// <summary>
        /// Thời gian nghỉ (phút) - Không tính lương
        /// </summary>
        public int BreakDurationMinutes { get; set; }

        /// <summary>
        /// Trạng thái ca: 0:Draft, 1:Published, 2:Completed, 3:Cancelled
        /// </summary>
        public int Status { get; set; }

        public string? Note { get; set; }

        // --- Audit & Concurrency ---
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        
        /// <summary>
        /// Optimistic Locking Version
        /// </summary>
        public int LockVersion { get; set; }

        // --- Navigation Properties ---
        public virtual Store Store { get; set; } = null!;
        public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
    }
}