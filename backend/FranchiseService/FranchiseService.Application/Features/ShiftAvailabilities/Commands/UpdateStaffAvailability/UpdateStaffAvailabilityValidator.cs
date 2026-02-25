using FluentValidation;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Commands.UpdateStaffAvailability;

public class UpdateStaffAvailabilityValidator : AbstractValidator<UpdateStaffAvailabilityCommand>
{
    public UpdateStaffAvailabilityValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        
        // Time Logic: EndTime > StartTime (if both provided)
        When(x => x.Request.StartTime.HasValue && x.Request.EndTime.HasValue, () =>
        {
            RuleFor(x => x.Request.EndTime)
                .GreaterThan(x => x.Request.StartTime).WithMessage("Gio ket thuc phai lon hon Gio bat dau");
        });
        
        // Neu client gui thay doi IsRecurring, bat buoc phai gui kem field tuong ung
        // De dam bao tinh toan ven du lieu (tranh truong hop chuyen Recurring=true ma van giu SpecificDate cu, thieu DayOfWeek)
        
        When(x => x.Request.IsRecurring.HasValue && x.Request.IsRecurring.Value == true, () =>
        {
            RuleFor(x => x.Request.DayOfWeek)
                .NotNull().WithMessage("Khi chuyen sang lich lap lai, phai cung cap Thu (DayOfWeek)");
        });

        When(x => x.Request.IsRecurring.HasValue && x.Request.IsRecurring.Value == false, () =>
        {
            RuleFor(x => x.Request.SpecificDate)
                .NotNull().WithMessage("Khi chuyen sang lich ngay cu the, phai cung cap Ngay (SpecificDate)");
        });
    }
}
