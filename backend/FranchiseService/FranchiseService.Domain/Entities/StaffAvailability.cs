using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities
{
    /// <summary>
    /// Lịch rảnh/bận của nhân viên.
    /// Bảng: staff_availabilities
    /// </summary>
    public class StaffAvailability : SoftDeletedEntity, IEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public bool? IsRecurring { get; set; }

        public DateOnly? SpecificDate { get; set; }

        public short? DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        /// <summary>
        /// True: Rảnh (Có thể làm), False: Bận (Xin nghỉ)
        /// </summary>
        public bool IsAvailable { get; set; }

        public string? Note { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}