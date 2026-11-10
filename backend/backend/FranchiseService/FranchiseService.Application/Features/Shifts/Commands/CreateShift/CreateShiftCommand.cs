using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.CreateShift
{
    public record CreateShiftCommand : IRequest<Guid>
    {
        /// <summary>
        /// ID Cửa hàng
        /// </summary>
        public Guid StoreId { get; set; }

        /// <summary>
        /// ID Mẫu ca (Optional)
        /// </summary>
        public Guid? ShiftTemplateId { get; set; }

        /// <summary>
        /// Ngày làm việc (VD: 2023-10-25)
        /// </summary>
        public DateOnly ShiftDate { get; set; }

        /// <summary>
        /// Giờ bắt đầu (VD: 08:00)
        /// </summary>
        public TimeOnly? StartTime { get; set; }

        /// <summary>
        /// Giờ kết thúc (VD: 17:00). Nếu nhỏ hơn StartTime -> Qua đêm.
        /// </summary>
        public TimeOnly? EndTime { get; set; }

        /// <summary>
        /// Số lượng nhân viên cần
        /// </summary>
        public int RequiredStaffCount { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Thời gian nghỉ (phút)
        /// </summary>
        public int? BreakDurationMinutes { get; set; }

        /// <summary>
        /// ID người tạo (Lấy từ Token/Context)
        /// </summary>
        public Guid CreatedBy { get; set; }
    }
}
