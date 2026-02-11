using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.RemoveStaff
{
    /// <summary>
    /// Command để xóa nhân viên khỏi ca làm việc (Soft Delete Assignment).
    /// FE: DELETE /shifts/{id}/assign/{userId}
    /// </summary>
    public record RemoveStaffCommand : IRequest<Unit>
    {
        /// <summary>
        /// ID ca làm việc
        /// </summary>
        public Guid ShiftId { get; set; }

        /// <summary>
        /// ID nhân viên cần xóa khỏi ca
        /// </summary>
        public Guid UserId { get; set; }
    }
}
