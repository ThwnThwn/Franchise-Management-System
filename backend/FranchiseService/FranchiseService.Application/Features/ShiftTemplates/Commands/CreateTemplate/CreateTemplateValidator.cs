using FluentValidation;

namespace FranchiseService.Application.Features.ShiftTemplates.Commands.CreateTemplate
{
    /// <summary>
    /// Validator cho lệnh tạo mẫu ca.
    /// </summary>
    public class CreateTemplateValidator : AbstractValidator<CreateTemplateCommand>
    {
        public CreateTemplateValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Mã mẫu ca (Code) không được để trống.")
                .MaximumLength(50).WithMessage("Mã mẫu ca không quá 50 ký tự.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên mẫu ca (Name) không được để trống.")
                .MaximumLength(200).WithMessage("Tên mẫu ca không quá 200 ký tự.");

            RuleFor(x => x.BreakDurationMinutes)
                .GreaterThanOrEqualTo(0).WithMessage("Thời gian nghỉ không được âm.");
        }
    }
}
