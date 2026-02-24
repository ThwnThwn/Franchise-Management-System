using FluentValidation;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Commands.CreateStaffAvailability;

public class CreateStaffAvailabilityValidator : AbstractValidator<CreateStaffAvailabilityCommand>
{
    public CreateStaffAvailabilityValidator()
    {
        RuleFor(x => x.Request.StartTime)
            .NotEmpty().WithMessage("Gio bat dau khong duoc de trong");

        RuleFor(x => x.Request.EndTime)
            .NotEmpty().WithMessage("Gio ket thuc khong duoc de trong")
            .GreaterThan(x => x.Request.StartTime).WithMessage("Gio ket thuc phai lon hon Gio bat dau");

        // Validate IsRecurring logic
        When(x => x.Request.IsRecurring, () =>
        {
            RuleFor(x => x.Request.DayOfWeek)
                .NotNull().WithMessage("Lich lap lai hang tuan phai chon Thu (DayOfWeek)");
            
            RuleFor(x => x.Request.SpecificDate)
                .Null().WithMessage("Lich lap lai khong duoc chon Ngay cu the");
        })
        .Otherwise(() => // IsRecurring = false
        {
            RuleFor(x => x.Request.SpecificDate)
                .NotNull().WithMessage("Lich khong lap lai phai chon Ngay cu the (SpecificDate)");
            
            RuleFor(x => x.Request.DayOfWeek)
                .Null().WithMessage("Lich khong lap lai khong duoc chon Thu (DayOfWeek)");
        });
    }
}
