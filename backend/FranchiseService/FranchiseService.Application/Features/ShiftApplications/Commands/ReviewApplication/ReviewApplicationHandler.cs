using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.ShiftApplications.Commands.ReviewApplication;

/// <summary>
/// Handler duyet hoac tu choi don dang ky nhan ca.
/// Khi Approve:
///   1. Re-validate nhan vien (phong truong hop lich thay doi tu luc Apply den luc Approve)
///   2. Kiem tra trung Assignment (phong truong hop da gan tay qua AssignStaff)
///   3. Kiem tra ca con slot trong khong
///   4. Tao ShiftAssignment tu dong
///   5. Neu du nguoi: chuyen Shift ve Published va auto-Reject cac don con lai
/// </summary>
public class ReviewApplicationHandler : IRequestHandler<ReviewApplicationCommand, bool>
{
    private readonly IGenericRepository<ShiftApplication> _applicationRepository;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;
    private readonly IGenericRepository<Shift> _shiftRepository;
    private readonly IShiftRuleService _shiftRuleService;
    private readonly IShiftValidationService _validationService;

    public ReviewApplicationHandler(
        IGenericRepository<ShiftApplication> applicationRepository,
        IGenericRepository<ShiftAssignment> assignmentRepository,
        IGenericRepository<Shift> shiftRepository,
        IShiftRuleService shiftRuleService,
        IShiftValidationService validationService)
    {
        _applicationRepository = applicationRepository;
        _assignmentRepository = assignmentRepository;
        _shiftRepository = shiftRepository;
        _shiftRuleService = shiftRuleService;
        _validationService = validationService;
    }

    public async Task<bool> Handle(
        ReviewApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Tim don dang ky
        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId)
            ?? throw new KeyNotFoundException(
                $"Khong tim thay don dang ky: {request.ApplicationId}");

        // Chi duyet don dang o trang thai Pending
        if (application.Status != (int)ShiftApplicationStatus.Pending)
            throw new InvalidOperationException("Don nay da duoc xu ly roi.");

        // Bat buoc nhap ly do khi tu choi
        if (!request.IsApproved && string.IsNullOrWhiteSpace(request.ReviewNote))
            throw new ArgumentException("Bat buoc nhap ly do khi tu choi don dang ky.");

        // 2. Lay thong tin Shift
        var shift = await _shiftRepository.GetByIdAsync(application.ShiftId)
            ?? throw new KeyNotFoundException("Khong tim thay ca lam viec.");

        if (request.IsApproved)
        {
            // 3. Kiem tra Shift con slot trong khong
            var currentAssignments = await _assignmentRepository.GetAllAsync(
                predicate: a => a.ShiftId == shift.Id);
            var assignmentList = currentAssignments.ToList();

            if (assignmentList.Count >= shift.RequiredStaffCount)
                throw new InvalidOperationException("Ca da du nguoi, khong the duyet them.");

            // 4. Kiem tra nhan vien da co Assignment chua (tranh trung voi gan tay)
            if (assignmentList.Any(a => a.UserId == application.UserId))
                throw new InvalidOperationException(
                    "Nhan vien nay da duoc phan cong ca nay roi (co the da gan tay truoc do).");

            // 5. Re-validate nghiep vu (lich co the thay doi tu luc Apply den luc Approve)
            var storeRule = await _shiftRuleService.GetEffectiveRuleAsync(shift.StoreId);
            int minHoursBreak = storeRule?.MinHoursBetweenShifts ?? 8;
            int maxHoursPerWeek = storeRule?.MaxHoursPerWeek ?? 48;

            var warnings = await _validationService.ValidateAssignment(
                shift, application.UserId, minHoursBreak, maxHoursPerWeek, cancellationToken);

            // Chi chan loi BLOCKING (trung ca). WARNING cho phep vi Manager dang chu dong duyet
            var blockingErrors = warnings.Where(w => w.Severity == "BLOCKING").ToList();
            if (blockingErrors.Any())
            {
                var errorMsg = string.Join("; ", blockingErrors.Select(e => e.Message));
                throw new InvalidOperationException(
                    $"Khong the duyet vi nhan vien vi pham: {errorMsg}");
            }

            // 6. Tao ShiftAssignment tu dong
            var assignment = new ShiftAssignment
            {
                Id = Guid.NewGuid(),
                ShiftId = application.ShiftId,
                UserId = application.UserId,
                Status = (int)ShiftAssignmentStatus.Assigned,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            await _assignmentRepository.AddAsync(assignment);

            // 7. Cap nhat don thanh Approved
            application.Status = (int)ShiftApplicationStatus.Approved;
            application.ReviewedAt = DateTimeOffset.UtcNow;
            application.ReviewedBy = request.ReviewedBy;
            application.ReviewNote = request.ReviewNote;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            await _applicationRepository.UpdateAsync(application);

            // 8. Kiem tra da du nguoi chua
            int newCount = assignmentList.Count + 1;
            if (newCount >= shift.RequiredStaffCount)
            {
                // Chuyen Shift ve Published (dong Cho Ca)
                shift.Status = (int)ShiftStatus.Published;
                shift.UpdatedAt = DateTimeOffset.UtcNow;
                await _shiftRepository.UpdateAsync(shift);

                // Tu dong tu choi tat ca don Pending con lai cua ca nay
                await AutoRejectRemainingApplications(
                    application.ShiftId, application.Id, request.ReviewedBy);
            }
        }
        else
        {
            // Reject: cap nhat trang thai don
            application.Status = (int)ShiftApplicationStatus.Rejected;
            application.ReviewedAt = DateTimeOffset.UtcNow;
            application.ReviewedBy = request.ReviewedBy;
            application.ReviewNote = request.ReviewNote;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            await _applicationRepository.UpdateAsync(application);
        }

        return true;
    }

    /// <summary>
    /// Tu dong tu choi tat ca don Pending con lai khi ca da du nguoi.
    /// Ghi ly do tu dong de Manager va nhan vien biet.
    /// </summary>
    private async Task AutoRejectRemainingApplications(
        Guid shiftId, Guid excludeAppId, Guid reviewedBy)
    {
        var remainingApps = await _applicationRepository.GetAllAsync(
            predicate: a => a.ShiftId == shiftId
                         && a.Status == (int)ShiftApplicationStatus.Pending
                         && a.Id != excludeAppId);

        foreach (var app in remainingApps)
        {
            app.Status = (int)ShiftApplicationStatus.Rejected;
            app.ReviewedAt = DateTimeOffset.UtcNow;
            app.ReviewedBy = reviewedBy;
            app.ReviewNote = "Tu dong tu choi: ca da du nguoi.";
            app.UpdatedAt = DateTimeOffset.UtcNow;
            await _applicationRepository.UpdateAsync(app);
        }
    }
}
