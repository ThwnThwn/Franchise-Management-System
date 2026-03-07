using FluentValidation;

namespace FranchiseService.Application.Features.Shifts.Commands.CheckIn;

/// <summary>
/// Validator cho CheckInCommand.
/// Kiem tra du lieu dau vao truoc khi Handler xu ly.
/// </summary>
public class CheckInValidator : AbstractValidator<CheckInCommand>
{
    public CheckInValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ma phan cong ca khong duoc de trong.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Ma nhan vien khong duoc de trong.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Vi do phai nam trong khoang -90 den 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Kinh do phai nam trong khoang -180 den 180.");
    }
}
