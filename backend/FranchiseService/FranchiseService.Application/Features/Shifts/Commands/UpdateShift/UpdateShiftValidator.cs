using FluentValidation;

namespace FranchiseService.Application.Features.Shifts.Commands.UpdateShift
{
    public class UpdateShiftValidator : AbstractValidator<UpdateShiftCommand>
    {
        public UpdateShiftValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            // Với Partial Update, các trường có thể null.
            // Chỉ validate nếu người dùng có gửi giá trị lên.

            RuleFor(x => x.ShiftDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)))
                .When(x => x.ShiftDate.HasValue)
                .WithMessage("Không thể chỉnh sửa ca làm việc quá cũ (chỉ cho phép sửa từ hôm qua).");

            RuleFor(x => x.RequiredStaffCount)
                .GreaterThanOrEqualTo(1)
                .When(x => x.RequiredStaffCount.HasValue)
                .WithMessage("Số lượng nhân viên phải lớn hơn hoặc bằng 1.");
        }
    }
}
