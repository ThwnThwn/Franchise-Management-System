using FranchiseService.Domain.Commons;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.UpdateShiftAssignment
{
    /// <summary>
    /// Lệnh đồng bộ danh sách nhân viên trong ca làm việc.
    /// FE gửi mảng userIds là kết quả cuối cùng, BE tự tính toán thêm hoặc xóa.
    /// </summary>
    public record UpdateShiftAssignmentCommand : IRequest<AssignStaffResult>
    {
        /// <summary>
        /// ID ca làm việc (được set từ route param)
        /// </summary>
        public Guid ShiftId { get; set; }

        /// <summary>
        /// Danh sách ID nhân viên cuối cùng sau khi Manager chỉnh sửa
        /// </summary>
        public List<Guid> UserIds { get; set; } = new();

        /// <summary>
        /// Cho phép ghi đè cảnh báo validation (FE gửi true sau khi Manager xác nhận)
        /// </summary>
        public bool ForceOverride { get; set; }

        /// <summary>
        /// Lý do ghi đè (bắt buộc khi ForceOverride = true)
        /// </summary>
        public string? OverrideReason { get; set; }

        /// <summary>
        /// ID của Manager thực hiện ghi đè
        /// </summary>
        public Guid? OverriddenBy { get; set; }
    }

    /// <summary>
    /// Kết quả trả về sau khi gán nhân viên.
    /// Nếu Success = true nhưng kèm Warnings thì FE hiện popup xác nhận ghi đè.
    /// </summary>
    public class AssignStaffResult
    {
        public bool Success { get; set; }
        public List<ValidationWarning> Warnings { get; set; } = new();
    }
}
