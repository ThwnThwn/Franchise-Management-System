using FluentValidation;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.RequestSwap;

/// <summary>
/// Kiem tra du lieu dau vao truoc khi gui yeu cau doi ca.
/// </summary>
public class RequestSwapValidator : AbstractValidator<RequestSwapCommand>
{
    public RequestSwapValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ma phan cong khong duoc de trong.");

        RuleFor(x => x.SourceUserId)
            .NotEmpty().WithMessage("Ma nguoi gui khong duoc de trong.");

        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Ma nguoi nhan khong duoc de trong.");

        RuleFor(x => x)
            .Must(x => x.SourceUserId != x.TargetUserId)
            .WithMessage("Khong the doi ca cho chinh minh.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Ly do doi ca khong duoc de trong.")
            .MaximumLength(500).WithMessage("Ly do toi da 500 ky tu.");
    }
}
