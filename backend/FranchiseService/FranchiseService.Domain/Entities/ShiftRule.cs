using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities
{
    /// <summary>
    /// Các quy tắc/luật lệ cho ca làm việc của Cửa hàng.
    /// Bảng: shift_rules
    /// </summary>
    public class ShiftRule : IEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// ID Cửa hàng áp dụng (Null = Global)
        /// </summary>
        public Guid? StoreId { get; set; }

        /// <summary>
        /// Cho phép chạy nhiều shop (shuttle)
        /// </summary>
        public bool AllowShuttle { get; set; }

        /// <summary>
        /// Cho phép đổi ca
        /// </summary>
        public bool AllowShiftSwap { get; set; }

        /// <summary>
        /// Thời gian nghỉ tối thiểu giữa 2 ca (giờ)
        /// </summary>
        public int MinHoursBetweenShifts { get; set; }

        /// <summary>
        /// Tổng số giờ làm tối đa trong tuần
        /// </summary>
        public int MaxHoursPerWeek { get; set; }

        public int GracePeriodMinutes { get; set; }
        public int DailyOtThresholdHours { get; set; }
        public int WeeklyOtThresholdHours { get; set; }


        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // --- Navigation Properties ---
        public virtual Store? Store { get; set; }
    }
}