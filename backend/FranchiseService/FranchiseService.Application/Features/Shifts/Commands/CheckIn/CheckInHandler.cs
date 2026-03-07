using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;
using SharedKernel.Constants;

namespace FranchiseService.Application.Features.Shifts.Commands.CheckIn;

/// <summary>
/// Handler xu ly Check-in.
/// Kiem tra quyen, trang thai ca, ap dung Grace Period, va tao ban ghi TimeLog.
/// </summary>
public class CheckInHandler : IRequestHandler<CheckInCommand, Guid>
{
    private readonly IGenericRepository<TimeLog> _timeLogRepository;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;
    private readonly IGenericRepository<Shift> _shiftRepository;
    private readonly IShiftRuleService _shiftRuleService;

    public CheckInHandler(
        IGenericRepository<TimeLog> timeLogRepository,
        IGenericRepository<ShiftAssignment> assignmentRepository,
        IGenericRepository<Shift> shiftRepository,
        IShiftRuleService shiftRuleService)
    {
        _timeLogRepository = timeLogRepository;
        _assignmentRepository = assignmentRepository;
        _shiftRepository = shiftRepository;
        _shiftRuleService = shiftRuleService;
    }

    public async Task<Guid> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate phan cong ton tai va thuoc ve user
        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId)
            ?? throw new KeyNotFoundException(
                $"Khong tim thay phan cong ca lam viec: {request.AssignmentId}");

        if (assignment.UserId != request.UserId)
            throw new UnauthorizedAccessException("Ban khong co quyen cham cong cho ca nay.");

        // 2. Kiem tra ca lam viec phai o trang thai Published moi cho phep check-in
        var shift = await _shiftRepository.GetByIdAsync(assignment.ShiftId)
            ?? throw new KeyNotFoundException("Khong tim thay ca lam viec.");

        if (shift.Status != (int)ShiftStatus.Published)
            throw new InvalidOperationException("Chi co the check-in khi ca lam viec da duoc phat hanh.");

        // 3. Chuyen doi gio ca lam viec tu gio Viet Nam sang UTC
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeConstants.DefaultTimeZoneId);

        var shiftLocalStart = shift.ShiftDate.ToDateTime(shift.StartTime);
        var shiftStartUtc = TimeZoneInfo.ConvertTimeToUtc(shiftLocalStart, vnTimeZone);

        var endShiftDate = shift.ShiftDate;
        if (shift.EndTime < shift.StartTime)
        {
            endShiftDate = endShiftDate.AddDays(1);
        }

        var shiftLocalEnd = endShiftDate.ToDateTime(shift.EndTime);
        var shiftEndUtc = TimeZoneInfo.ConvertTimeToUtc(shiftLocalEnd, vnTimeZone);

        var now = DateTime.UtcNow;

        // 4. Validate: khong cho check-in qua som (truoc gio bat dau ca toi da 30 phut)
        var earliestAllowed = shiftStartUtc.AddMinutes(-TimeConstants.MaxEarlyCheckInMinutes);
        if (now < earliestAllowed)
            throw new InvalidOperationException(
                $"Chua den gio check-in. Ban chi co the check-in som nhat {TimeConstants.MaxEarlyCheckInMinutes} phut truoc gio bat dau ca.");

        // 5. Validate: khong cho check-in sau khi ca da ket thuc
        if (now > shiftEndUtc)
            throw new InvalidOperationException("Ca lam viec da ket thuc, khong the check-in.");

        // 6. Kiem tra chua check-in truoc do de tranh tao trung TimeLog
        var existingLogs = await _timeLogRepository.GetAllAsync(
            predicate: tl => tl.ShiftAssignmentId == request.AssignmentId);

        if (existingLogs.Any(tl => tl.CheckOutAt == null))
            throw new InvalidOperationException("Ban da check-in roi. Hay check-out truoc.");

        // 7. Xu ly Grace Period: lay Rule hieu luc cua Store
        var storeRule = await _shiftRuleService.GetEffectiveRuleAsync(shift.StoreId);
        int gracePeriod = storeRule?.GracePeriodMinutes ?? 0;

        var effectiveCheckIn = now;

        // Neu nhan vien di tre nhung van trong khoang cho phep (Grace Period)
        // thi he thong lam tron ve dung gio bat dau ca (tranh tru luong oan)
        if (now > shiftStartUtc && (now - shiftStartUtc).TotalMinutes <= gracePeriod)
        {
            effectiveCheckIn = shiftStartUtc;
        }

        // 8. Tao TimeLog moi
        var timeLog = new TimeLog
        {
            Id = Guid.NewGuid(),
            ShiftAssignmentId = request.AssignmentId,
            CheckInAt = effectiveCheckIn,
            CheckInLat = request.Latitude,
            CheckInLong = request.Longitude,
            Status = (int)TimeLogStatus.Processing,
            ManagerNote = request.Note,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _timeLogRepository.AddAsync(timeLog);

        return timeLog.Id;
    }
}