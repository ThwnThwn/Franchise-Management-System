using FluentValidation;

namespace FranchiseService.Application.Features.ShiftRules.Commands.SaveShiftRule;

public class SaveShiftRuleValidator : AbstractValidator<SaveShiftRuleCommand>
{
    public SaveShiftRuleValidator()
    {
        // BUSINESS LOGIC: Valication (Task 3.6)
        
        RuleFor(x => x.Request.MinHoursBetweenShifts)
            .InclusiveBetween(0, 24)
            .WithMessage("Thoi gian nghi toi thieu phai tu 0 den 24 gio");

        RuleFor(x => x.Request.MaxHoursPerWeek)
            .GreaterThan(0)
            .LessThan(168)
            .WithMessage("Gio lam viec toi da trong tuan phai > 0 va < 168 (Tong gio trong tuan)");
    }
}
