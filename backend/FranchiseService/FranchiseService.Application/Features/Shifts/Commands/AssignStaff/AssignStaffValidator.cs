using FluentValidation;

namespace FranchiseService.Application.Features.Shifts.Commands.AssignStaff
{
    /// <summary>
    /// Validator cho lệnh gán nhân viên vào ca làm việc (batch mode).
    /// Kiểm tra đầu vào cơ bản trước khi vào Handler logic.
    /// </summary>
    public class AssignStaffValidator : AbstractValidator<AssignStaffCommand>
    {
        public AssignStaffValidator()
        {
            // Danh sách UserIds không được rỗng
            RuleFor(x => x.UserIds)
                .NotEmpty()
                .WithMessage("Danh sách nhân viên (UserIds) không được để trống.");

            // Mỗi UserId trong danh sách phải hợp lệ (không phải Guid.Empty)
            RuleForEach(x => x.UserIds)
                .NotEmpty()
                .WithMessage("UserId không được là giá trị rỗng.");

            // Không cho phép trùng userId trong cùng 1 request
            RuleFor(x => x.UserIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Danh sách UserIds không được có giá trị trùng lặp.");
        }
    }
}
