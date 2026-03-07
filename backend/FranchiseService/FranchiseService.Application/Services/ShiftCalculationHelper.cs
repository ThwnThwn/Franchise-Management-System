using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Services;

/// <summary>
/// Cac ham tinh toan dung chung cho ca lam viec.
/// Tap trung utility 1 cho de tai su dung, tranh trung lap code.
/// Dung trong: ShiftValidationService, AutoFillHandler, GetAvailableStaffs, GetApplicationsByShift.
/// </summary>
public static class ShiftCalculationHelper
{
    /// <summary>
    /// Kiem tra 2 khung gio co trung nhau khong. Ho tro ca dem (qua nua dem).
    /// </summary>
    public static bool IsOverlap(TimeOnly s1, TimeOnly e1, TimeOnly s2, TimeOnly e2)
    {
        int start1 = s1.Hour * 60 + s1.Minute, end1 = e1.Hour * 60 + e1.Minute;
        if (end1 <= start1) end1 += 1440;
        int start2 = s2.Hour * 60 + s2.Minute, end2 = e2.Hour * 60 + e2.Minute;
        if (end2 <= start2) end2 += 1440;
        return start1 < end2 && end1 > start2;
    }

    /// <summary>
    /// Tinh so gio thuc cua ca (tru thoi gian nghi). Ho tro ca dem.
    /// </summary>
    public static double CalculateNetHours(TimeOnly start, TimeOnly end, int breakMinutes)
    {
        double grossHours;
        if (end <= start)
            grossHours = (1440 - (start.Hour * 60 + start.Minute) + (end.Hour * 60 + end.Minute)) / 60.0;
        else
            grossHours = (end.Hour * 60 + end.Minute - start.Hour * 60 - start.Minute) / 60.0;
        return Math.Max(0, grossHours - (breakMinutes / 60.0));
    }

    /// <summary>
    /// Tim ngay Thu Hai cua tuan chua ngay truyen vao.
    /// Dung de xac dinh khoang tuan khi tinh tong gio lam.
    /// </summary>
    public static DateOnly GetMondayOfWeek(DateOnly date)
    {
        int daysSinceMonday = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (daysSinceMonday < 0) daysSinceMonday += 7;
        return date.AddDays(-daysSinceMonday);
    }

    /// <summary>
    /// Kiem tra nhan vien co bao BAN trung gio voi ca hay khong.
    /// Khong co du lieu lich ranh ban thi coi nhu ranh (mac dinh).
    /// </summary>
    public static bool IsStaffBusy(
        Shift shift, Guid userId,
        Dictionary<Guid, List<StaffAvailability>> availsByUser)
    {
        if (!availsByUser.TryGetValue(userId, out var avails))
            return false;

        return avails.Any(a =>
            !a.IsAvailable
            && MatchesDate(a, shift.ShiftDate)
            && IsOverlap(shift.StartTime, shift.EndTime, a.StartTime, a.EndTime));
    }

    /// <summary>
    /// Loc danh sach phan cong cua nhan vien trong 1 tuan cu the (Thu Hai den Chu Nhat).
    /// Dam bao tinh tong gio lam dung theo tuan, khong bi lon khi khoang ngay vuot 2 tuan.
    /// </summary>
    public static List<ShiftAssignment> GetAssignmentsInWeek(
        Guid userId, DateOnly weekStart, DateOnly weekEnd,
        Dictionary<Guid, List<ShiftAssignment>> assignsByUser)
    {
        if (!assignsByUser.TryGetValue(userId, out var allAssigns))
            return new List<ShiftAssignment>();

        return allAssigns
            .Where(a => a.Shift != null
                     && a.Shift.ShiftDate >= weekStart
                     && a.Shift.ShiftDate <= weekEnd)
            .ToList();
    }

    /// <summary>
    /// Kiem tra lich ranh ban co khop ngay voi ca khong.
    /// Ho tro ca lich lap lai (theo thu trong tuan) va lich cu the (theo ngay).
    /// </summary>
    public static bool MatchesDate(StaffAvailability avail, DateOnly shiftDate)
    {
        if (avail.IsRecurring == true)
            return avail.DayOfWeek == (short)shiftDate.DayOfWeek;
        return avail.SpecificDate == shiftDate;
    }
}
