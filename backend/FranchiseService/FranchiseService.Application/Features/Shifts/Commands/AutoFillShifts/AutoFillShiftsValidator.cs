using FluentValidation;

namespace FranchiseService.Application.Features.Shifts.Commands.AutoFillShifts;

/// <summary>
/// Kiem tra du lieu dau vao truoc khi chay tu dong xep ca.
/// </summary>
public class AutoFillShiftsValidator : AbstractValidator<AutoFillShiftsCommand>
{
    public AutoFillShiftsValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Ma cua hang khong duoc de trong.");

        RuleFor(x => x.StartDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Ngay bat dau phai tu hom nay tro di.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Ngay ket thuc phai lon hon hoac bang ngay bat dau.");

        RuleFor(x => x)
            .Must(x => x.EndDate.DayNumber - x.StartDate.DayNumber <= 14)
            .WithMessage("Khoang thoi gian toi da la 14 ngay.");
    }
}
