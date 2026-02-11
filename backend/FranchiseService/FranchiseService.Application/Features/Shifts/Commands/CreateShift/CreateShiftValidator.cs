using FluentValidation;

namespace FranchiseService.Application.Features.Shifts.Commands.CreateShift
{
    public class CreateShiftValidator : AbstractValidator<CreateShiftCommand>
    {
        public CreateShiftValidator()
        {
            RuleFor(x => x.StoreId)
                .NotEmpty().WithMessage("StoreId không được để trống.");

            RuleFor(x => x.ShiftDate)
                .NotEmpty().WithMessage("Ngày làm việc không được để trống.")
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Không thể tạo ca làm việc cho quá khứ.");

            RuleFor(x => x.RequiredStaffCount)
                .GreaterThanOrEqualTo(1).WithMessage("Số lượng nhân viên phải lớn hơn hoặc bằng 1.");

            // Nếu KHÔNG có TemplateId thì BẮT BUỘC phải nhập StartTime/EndTime
            RuleFor(x => x.StartTime)
                .NotEmpty().When(x => x.ShiftTemplateId == null)
                .WithMessage("Giờ bắt đầu là bắt buộc (trừ khi chọn Mẫu ca).");

            RuleFor(x => x.EndTime)
                .NotEmpty().When(x => x.ShiftTemplateId == null)
                .WithMessage("Giờ kết thúc là bắt buộc (trừ khi chọn Mẫu ca).");

            // Lưu ý: Không validate StartTime < EndTime vì hỗ trợ ca qua đêm.
            // Tuy nhiên, có thể validate độ dài ca làm việc (VD: không quá 24h) nếu cần logic phức tạp hơn.
        }
    }
}
