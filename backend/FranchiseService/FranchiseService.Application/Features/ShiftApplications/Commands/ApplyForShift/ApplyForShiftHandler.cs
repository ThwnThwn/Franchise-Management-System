using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;
using SharedKernel.Constants;

namespace FranchiseService.Application.Features.ShiftApplications.Commands.ApplyForShift;

/// <summary>
/// Handler xu ly don dang ky nhan ca tu Cho Ca.
/// Quy trinh:
///   1. Kiem tra ca phai o trang thai Open
///   2. Kiem tra ca chua qua ngay
///   3. Kiem tra nhan vien chua dang ky ca nay truoc do
///   4. Validate nghiep vu (trung ca, nghi chua du, lo gio tuan)
///   5. Chan tat ca vi pham (khong co Override o Cho Ca)
///   6. Tao don Pending neu hop le
/// </summary>
public class ApplyForShiftHandler : IRequestHandler<ApplyForShiftCommand, Guid>
{
    private readonly IGenericRepository<Shift> _shiftRepository;
    private readonly IGenericRepository<ShiftApplication> _applicationRepository;
    private readonly IShiftRuleService _shiftRuleService;
    private readonly IShiftValidationService _validationService;

    public ApplyForShiftHandler(
        IGenericRepository<Shift> shiftRepository,
        IGenericRepository<ShiftApplication> applicationRepository,
        IShiftRuleService shiftRuleService,
        IShiftValidationService validationService)
    {
        _shiftRepository = shiftRepository;
        _applicationRepository = applicationRepository;
        _shiftRuleService = shiftRuleService;
        _validationService = validationService;
    }

    public async Task<Guid> Handle(ApplyForShiftCommand request, CancellationToken cancellationToken)
    {
        // 1. Tim ca lam viec
        var shift = await _shiftRepository.GetByIdAsync(request.ShiftId)
            ?? throw new KeyNotFoundException(
                $"Khong tim thay ca lam viec: {request.ShiftId}");

        // 2. Ca phai dang o trang thai Open (mo Cho Ca)
        if (shift.Status != (int)ShiftStatus.Open)
            throw new InvalidOperationException(
                "Ca nay khong dang mo cho dang ky. Chi ca co trang thai Open moi cho phep.");

        // 3. Ca phai chua dien ra (ngay >= hom nay, tinh theo gio VN)
        var today = GetTodayInVietnam();
        if (shift.ShiftDate < today)
            throw new InvalidOperationException("Khong the dang ky ca da qua ngay.");

        // 4. Kiem tra nhan vien da dang ky ca nay chua (tranh trung don)
        var existingApps = await _applicationRepository.GetAllAsync(
            predicate: a => a.ShiftId == request.ShiftId
                         && a.UserId == request.UserId);

        if (existingApps.Any())
            throw new InvalidOperationException("Ban da dang ky ca nay roi. Khong the dang ky lan nua.");

        // 5. Validate nghiep vu: trung ca, nghi chua du, lo gio tuan
        //    Cho Ca KHONG co co che Override -> chan tat ca vi pham (ke ca WARNING)
        var storeRule = await _shiftRuleService.GetEffectiveRuleAsync(shift.StoreId);
        int minHoursBreak = storeRule?.MinHoursBetweenShifts ?? 8;
        int maxHoursPerWeek = storeRule?.MaxHoursPerWeek ?? 48;

        var warnings = await _validationService.ValidateAssignment(
            shift, request.UserId, minHoursBreak, maxHoursPerWeek, cancellationToken);

        // Tu choi neu co bat ky vi pham nao (BLOCKING hoac WARNING deu khong cho qua)
        if (warnings.Any())
        {
            var errorMessages = string.Join("; ", warnings.Select(w => w.Message));
            throw new InvalidOperationException($"Khong the dang ky: {errorMessages}");
        }

        // 6. Tao don dang ky moi voi trang thai Pending
        var application = new ShiftApplication
        {
            Id = Guid.NewGuid(),
            ShiftId = request.ShiftId,
            UserId = request.UserId,
            Status = (int)ShiftApplicationStatus.Pending,
            Note = request.Note,
            AppliedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _applicationRepository.AddAsync(application);

        return application.Id;
    }

    private static DateOnly GetTodayInVietnam()
    {
        var vnTz = TimeZoneInfo.FindSystemTimeZoneById(TimeConstants.DefaultTimeZoneId);
        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz);
        return DateOnly.FromDateTime(nowVn);
    }
}
