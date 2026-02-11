using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.UpdateShift
{
    public record UpdateShiftCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }

        public Guid? StoreId { get; set; }

        public Guid? ShiftTemplateId { get; set; }

        public DateOnly? ShiftDate { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public int? RequiredStaffCount { get; set; }

        public string? Note { get; set; }

        public int? BreakDurationMinutes { get; set; }

        /// <summary>
        /// Version để check Optimistic Locking
        /// </summary>
        public int LockVersion { get; set; }

        /// <summary>
        /// Trạng thái ca (optional): 0:Draft, 1:Published, 2:Completed, 3:Cancelled.
        /// Dùng khi FE muốn đổi status qua update (VD: Draft → Published).
        /// </summary>
        public int? Status { get; set; }

        public Guid UpdatedBy { get; set; }
    }
}
