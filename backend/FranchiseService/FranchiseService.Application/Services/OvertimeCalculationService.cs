using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using SharedKernel.Constants;
using System.Linq.Expressions;

namespace FranchiseService.Application.Services;

/// <summary>
/// Service tinh toan Overtime.
/// Cong thuc:
///   1. totalWorkedMinutes = CheckOutAt - EffectiveStart - BreakDuration
///   2. RegularMinutes = Min(totalWorked, dailyThreshold)
///   3. DailyOtMinutes = Max(0, totalWorked - dailyThreshold)
///   4. WeeklyOtMinutes = phan Regular vuot qua weeklyThreshold
/// </summary>
public class OvertimeCalculationService : IOvertimeCalculationService
{
    private readonly IGenericRepository<TimeLog> _timeLogRepository;

    public OvertimeCalculationService(IGenericRepository<TimeLog> timeLogRepository)
    {
        _timeLogRepository = timeLogRepository;
    }

    public async Task<OvertimeResult> CalculateAsync(
        TimeLog timeLog, Shift shift, ShiftRule? rule, Guid userId)
    {
        // Gia tri mac dinh neu khong co Rule
        int dailyThresholdHours = rule?.DailyOtThresholdHours ?? 8;
        int weeklyThresholdHours = rule?.WeeklyOtThresholdHours ?? 40;

        // Tinh moc bat dau thuc te: lay Max(CheckInAt, ShiftStartTime)
        // De tranh tinh luong cho viec di som truoc gio ca
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeConstants.DefaultTimeZoneId);
        var shiftLocalTime = shift.ShiftDate.ToDateTime(shift.StartTime);
        var shiftStartUtc = TimeZoneInfo.ConvertTimeToUtc(shiftLocalTime, vnTimeZone);

        var effectiveStart = new DateTime(
            Math.Max(timeLog.CheckInAt!.Value.Ticks, shiftStartUtc.Ticks),
            DateTimeKind.Utc);

        // Tinh tong so phut lam viec thuc te
        int totalWorkedMinutes = (int)(timeLog.CheckOutAt!.Value - effectiveStart).TotalMinutes;

        // Tru thoi gian nghi (break) tuy theo cau hinh ca
        totalWorkedMinutes -= shift.BreakDurationMinutes;
        if (totalWorkedMinutes < 0) totalWorkedMinutes = 0;

        // Buoc 1: Tinh Daily OT
        int maxRegularPerDay = dailyThresholdHours * 60;
        int regularThisShift = Math.Min(totalWorkedMinutes, maxRegularPerDay);
        int dailyOtThisShift = Math.Max(0, totalWorkedMinutes - maxRegularPerDay);

        // Buoc 2: Tinh Weekly OT
        // Dung ShiftCalculationHelper.GetMondayOfWeek thay vi tinh truc tiep
        var startOfWeek = ShiftCalculationHelper.GetMondayOfWeek(shift.ShiftDate);
        var endOfWeek = startOfWeek.AddDays(6);

        // Truy van tat ca TimeLogs da duoc duyet (Valid) cua user trong tuan nay
        // Loai tru ban ghi hien tai de tranh tinh trung
        var weeklyLogs = await _timeLogRepository.GetAllAsync(
            includes: new Expression<Func<TimeLog, object>>[]
            {
                tl => tl.ShiftAssignment,
                tl => tl.ShiftAssignment.Shift
            },
            predicate: tl =>
                tl.ShiftAssignment.UserId == userId &&
                tl.CheckOutAt != null &&
                tl.Status == (int)TimeLogStatus.Valid &&
                tl.ShiftAssignment.Shift.ShiftDate >= startOfWeek &&
                tl.ShiftAssignment.Shift.ShiftDate <= endOfWeek &&
                tl.Id != timeLog.Id
        );

        // Tong so phut Regular da tich luy trong tuan (truoc ca hien tai)
        int workedRegularThisWeek = weeklyLogs.Sum(tl => tl.RegularMinutes);
        int maxRegularPerWeek = weeklyThresholdHours * 60;

        int weeklyOtThisShift = 0;

        // Kiem tra neu Regular moi cong don lam vuot gioi han tuan
        if (workedRegularThisWeek + regularThisShift > maxRegularPerWeek)
        {
            int overWeekLimit = (workedRegularThisWeek + regularThisShift) - maxRegularPerWeek;

            // Phan vuot khong duoc lon hon regular cua chinh ca nay
            weeklyOtThisShift = Math.Min(overWeekLimit, regularThisShift);

            // Giam regular tuong ung de chuyen sang Weekly OT
            regularThisShift -= weeklyOtThisShift;
        }

        return new OvertimeResult
        {
            RegularMinutes = regularThisShift,
            DailyOtMinutes = dailyOtThisShift,
            WeeklyOtMinutes = weeklyOtThisShift
        };
    }
}
