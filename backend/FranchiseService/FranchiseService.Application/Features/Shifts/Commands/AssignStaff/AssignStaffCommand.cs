using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.AssignStaff
{
    /// <summary>
    /// Command gán nhân viên vào ca làm việc (hỗ trợ batch).
    /// FE gửi: POST /shifts/{id}/assign { userIds: [...] }
    /// </summary>
    public record AssignStaffCommand : IRequest<Unit>
    {
        /// <summary>
        /// ID ca làm việc (set từ route param bởi Controller)
        /// </summary>
        public Guid ShiftId { get; set; }

        /// <summary>
        /// Danh sách ID nhân viên cần gán vào ca.
        /// FE gửi mảng string[], BE parse thành List Guid.
        /// </summary>
        public List<Guid> UserIds { get; set; } = new();
    }
}
