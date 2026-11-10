using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.UpdateTimeLog;

/// <summary>
/// Handler cho Manager duyet cong.
/// Cap nhat trang thai cham cong, ly do, va tinh lai OT day du neu chuyen ve Valid.
/// </summary>
public class UpdateTimeLogHandler : IRequestHandler<UpdateTimeLogCommand, bool>
{
    private readonly IGenericRepository<TimeLog> _timeLogRepository;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;
    private readonly IGenericRepository<Shift> _shiftRepository;
    private readonly IShiftRuleService _shiftRuleService;
    private readonly IOvertimeCalculationService _overtimeService;

    public UpdateTimeLogHandler(
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

    public async Task<bool> Handle(UpdateTimeLogCommand request, CancellationToken cancellationToken)
    {
        // Kiem tra trang thai truyen vao phai la gia tri hop le trong enum
        if (!Enum.IsDefined(typeof(TimeLogStatus), request.Status))
            throw new ArgumentException("Trang thai khong hop le. Chi chap nhan cac gia tri tu 0 den 3.");

        // Bat buoc quan ly phai nhap ly do khi tu choi ban ghi cham cong
        if (request.Status == (int)TimeLogStatus.Invalid && string.IsNullOrWhiteSpace(request.ManagerNote))
            throw new ArgumentException("Bat buoc phai nhap ly do khi tu choi ban ghi cham cong.");

        var timeLog = await _timeLogRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Khong tim thay ban ghi cham cong: {request.Id}");

        // Cap nhat trang thai va ghi chu duyet cong do quan ly nhap
        timeLog.Status = request.Status;
        timeLog.ManagerNote = request.ManagerNote;
        timeLog.UpdatedAt = DateTimeOffset.UtcNow;

        // Khi Manager duyet lai cong ve trang thai Valid va co du thong tin check-in/check-out,
        // tinh lai day du OT (Daily va Weekly) thay vi chi tinh Regular nhu truoc.
        if (request.Status == (int)TimeLogStatus.Valid
            && timeLog.CheckInAt.HasValue
            && timeLog.CheckOutAt.HasValue)
        {
            // Lay thong tin Shift lien ket de tinh toan OT
            var assignment = await _assignmentRepository.GetByIdAsync(timeLog.ShiftAssignmentId)
                ?? throw new KeyNotFoundException("Khong tim thay phan cong ca lien ket.");

            var shift = await _shiftRepository.GetByIdAsync(assignment.ShiftId)
                ?? throw new KeyNotFoundException("Khong tim thay ca lam viec lien ket.");

            var storeRule = await _shiftRuleService.GetEffectiveRuleAsync(shift.StoreId);

            // Goi service tinh OT dung chung (cung logic voi CheckOutHandler)
            var otResult = await _overtimeService.CalculateAsync(
                timeLog, shift, storeRule, assignment.UserId);

            timeLog.RegularMinutes = otResult.RegularMinutes;
            timeLog.DailyOtMinutes = otResult.DailyOtMinutes;
            timeLog.WeeklyOtMinutes = otResult.WeeklyOtMinutes;
        }

        await _timeLogRepository.UpdateAsync(timeLog);
        return true;
    }
}
