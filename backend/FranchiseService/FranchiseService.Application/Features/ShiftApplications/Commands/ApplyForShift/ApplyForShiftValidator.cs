using FluentValidation;

namespace FranchiseService.Application.Features.ShiftApplications.Commands.ApplyForShift;

/// <summary>
/// Kiem tra du lieu dau vao truoc khi xu ly don dang ky nhan ca.
/// </summary>
public class ApplyForShiftValidator : AbstractValidator<ApplyForShiftCommand>
{
    public ApplyForShiftValidator()
    {
        RuleFor(x => x.ShiftId)
            .NotEmpty().WithMessage("Ma ca lam viec khong duoc de trong.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Ma nhan vien khong duoc de trong.");
    }
}
