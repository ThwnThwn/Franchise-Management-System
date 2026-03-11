using FluentValidation;
using SharedKernel.Constants;

namespace FranchiseService.Application.Features.Shifts.Commands.AutoFillShifts;

/// <summary>
/// Kiem tra du lieu dau vao truoc khi chay tu dong xep ca.
/// "Hom nay" tinh theo mui gio Viet Nam de khop voi ngay hien thi tren UI.
/// </summary>
public class AutoFillShiftsValidator : AbstractValidator<AutoFillShiftsCommand>
{
    public AutoFillShiftsValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Ma cua hang khong duoc de trong.");

        RuleFor(x => x.StartDate)
            .Must(d => d >= GetTodayInVietnam())
            .WithMessage("Ngay bat dau phai tu hom nay tro di.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Ngay ket thuc phai lon hon hoac bang ngay bat dau.");

        RuleFor(x => x)
            .Must(x => x.EndDate.DayNumber - x.StartDate.DayNumber <= 14)
            .WithMessage("Khoang thoi gian toi da la 14 ngay.");
    }

    /// <summary>
    /// Lay ngay hom nay theo mui gio Viet Nam (UTC+7) de dong bo voi UI va quy tac nghiep vu.
    /// </summary>
    private static DateOnly GetTodayInVietnam()
    {
        var vnTz = TimeZoneInfo.FindSystemTimeZoneById(TimeConstants.DefaultTimeZoneId);
        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz);
        return DateOnly.FromDateTime(nowVn);
    }
}
