using FranchiseService.Application.Constants;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;
using SharedKernel.Constants;

namespace FranchiseService.Application.Features.Shifts.Commands.CheckOut;

/// <summary>
/// Handler xu ly Check-out.
/// Cap nhat TimeLog: thoi gian checkout, toa do, va tinh toan phan bo OT.
/// </summary>
public class CheckOutHandler : IRequestHandler<CheckOutCommand, bool>
{
    private readonly IGenericRepository<TimeLog> _timeLogRepository;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;
    private readonly IGenericRepository<Shift> _shiftRepository;
    private readonly IShiftRuleService _shiftRuleService;
    private readonly IOvertimeCalculationService _overtimeService;

    public CheckOutHandler(
        IGenericRepository<TimeLog> timeLogRepository,
        IGenericRepository<ShiftAssignment> assignmentRepository,
        IGenericRepository<Shift> shiftRepository,
        IShiftRuleService shiftRuleService,
        IOvertimeCalculationService overtimeService)
    {
        _timeLogRepository = timeLogRepository;
        _assignmentRepository = assignmentRepository;
        _shiftRepository = shiftRepository;
        _shiftRuleService = shiftRuleService;
        _overtimeService = overtimeService;
    }

    public async Task<bool> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        // 1. Tim ban ghi TimeLog
        var timeLog = await _timeLogRepository.GetByIdAsync(request.LogId)
             ?? throw new KeyNotFoundException($"Khong tim thay ban ghi cham cong: {request.LogId}");

        // 2. Validate quyen so huu qua ban ghi ShiftAssignment
        var assignment = await _assignmentRepository.GetByIdAsync(timeLog.ShiftAssignmentId)
             ?? throw new KeyNotFoundException("Khong tim thay phan cong ca.");

        if (assignment.UserId != request.UserId)
            throw new UnauthorizedAccessException("Ban khong co quyen check-out ca nay.");

        // 3. Kiem tra xem nguoi dung da check-out chua
        if (timeLog.CheckOutAt.HasValue)
            throw new InvalidOperationException("Ban da check-out roi.");

        // 4. Validate: khong cho check-out neu chua co check-in
        if (!timeLog.CheckInAt.HasValue)
            throw new InvalidOperationException("Chua co thong tin check-in, khong the check-out.");

        // 5. Tai thong tin Shift de tinh toan
        var shift = await _shiftRepository.GetByIdAsync(assignment.ShiftId)
            ?? throw new KeyNotFoundException("Khong tim thay ca lam viec lien ket.");

        // 6. Gioi han thoi diem check-out: khong cho vuot qua X gio sau ket thuc ca
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId());

        var endShiftDate = shift.ShiftDate;
        if (shift.EndTime < shift.StartTime)
        {
            endShiftDate = endShiftDate.AddDays(1);
        }

        var shiftEndLocal = endShiftDate.ToDateTime(shift.EndTime);
        var shiftEndUtc = TimeZoneInfo.ConvertTimeToUtc(shiftEndLocal, vnTimeZone);
        var maxCheckOutTime = shiftEndUtc.AddHours(TimeConstants.MaxLateCheckOutHours);

        var now = DateTime.UtcNow;

        // Cap thoi diem check-out: neu vuot qua gioi han thi gioi han lai
        var effectiveCheckOut = now > maxCheckOutTime ? maxCheckOutTime : now;

        // 7. Cap nhat thong tin Check-out
        timeLog.CheckOutAt = effectiveCheckOut;
        timeLog.CheckOutLat = request.Latitude;
        timeLog.CheckOutLong = request.Longitude;
        timeLog.UpdatedAt = DateTimeOffset.UtcNow;

        // 8. Gop note check-out vao ghi chu hien co (neu co)
        if (!string.IsNullOrEmpty(request.Note))
        {
            timeLog.ManagerNote = string.IsNullOrEmpty(timeLog.ManagerNote)
                ? request.Note
                : $"{timeLog.ManagerNote} | Note checkout: {request.Note}";
        }

        // 9. Tinh toan phan bo Overtime bang service dung chung
        var storeRule = await _shiftRuleService.GetEffectiveRuleAsync(shift.StoreId);
        var otResult = await _overtimeService.CalculateAsync(
            timeLog, shift, storeRule, request.UserId);

        timeLog.RegularMinutes = otResult.RegularMinutes;
        timeLog.DailyOtMinutes = otResult.DailyOtMinutes;
        timeLog.WeeklyOtMinutes = otResult.WeeklyOtMinutes;

        // 10. Chuyen trang thai sang Valid sau khi hoan tat check-out
        timeLog.Status = (int)TimeLogStatus.Valid;

        await _timeLogRepository.UpdateAsync(timeLog);

        return true;
    }
}