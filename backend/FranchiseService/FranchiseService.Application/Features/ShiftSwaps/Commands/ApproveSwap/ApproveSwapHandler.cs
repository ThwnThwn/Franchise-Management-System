using FranchiseService.Application.Features.ShiftSwaps.Helpers;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.ApproveSwap;

/// <summary>
/// Xu ly Manager duyet yeu cau doi ca.
///
/// Neu duyet: validate lai cho B, neu OK thi swap UserId trong Assignment.
/// Dung RejectSwap helper cho tat ca truong hop tu choi.
/// </summary>
public class ApproveSwapHandler : IRequestHandler<ApproveSwapCommand, bool>
{
    private readonly IGenericRepository<ShiftSwapRequest> _swapRepo;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepo;
    private readonly IGenericRepository<Shift> _shiftRepo;
    private readonly IShiftValidationService _validationService;
    private readonly IShiftRuleService _shiftRuleService;

    public ApproveSwapHandler(
        IGenericRepository<ShiftSwapRequest> swapRepo,
        IGenericRepository<ShiftAssignment> assignmentRepo,
        IGenericRepository<Shift> shiftRepo,
        IShiftValidationService validationService,
        IShiftRuleService shiftRuleService)
    {
        _swapRepo = swapRepo;
        _assignmentRepo = assignmentRepo;
        _shiftRepo = shiftRepo;
        _validationService = validationService;
        _shiftRuleService = shiftRuleService;
    }

    public async Task<bool> Handle(ApproveSwapCommand request, CancellationToken ct)
    {
        // 1. Tim yeu cau va kiem tra trang thai
        var swap = await _swapRepo.GetByIdAsync(request.SwapRequestId);
        if (swap == null)
            throw new InvalidOperationException("Yeu cau doi ca khong ton tai.");
        if (swap.Status != (int)SwapRequestStatus.PendingManager)
            throw new InvalidOperationException(
                "Yeu cau khong o trang thai cho Manager duyet.");

        // Manager tu choi
        if (!request.Approve)
        {
            SwapValidationHelper.RejectSwap(swap, request.ReviewedBy,
                request.RejectReason ?? "Manager tu choi.");
            await _swapRepo.UpdateAsync(swap);
            return false;
        }

        // 2. Assignment cua A van ton tai
        var assignment = await _assignmentRepo.GetByIdAsync(swap.SourceAssignmentId);
        if (assignment == null)
        {
            SwapValidationHelper.RejectSwap(swap, request.ReviewedBy,
                "Phan cong goc da bi xoa.");
            await _swapRepo.UpdateAsync(swap);
            throw new InvalidOperationException("Phan cong goc khong con ton tai.");
        }

        // 3. Lay thong tin ca va rule
        var shift = await _shiftRepo.GetByIdAsync(assignment.ShiftId);
        if (shift == null)
            throw new InvalidOperationException("Ca lam viec khong ton tai.");

        // 3b. Kiem tra ca chua bat dau (so sanh ca ngay va gio)
        var hienTaiVn = DateTime.UtcNow.AddHours(7);
        var thoiDiemBatDauCa = shift.ShiftDate.ToDateTime(shift.StartTime);
        if (hienTaiVn >= thoiDiemBatDauCa)
        {
            SwapValidationHelper.RejectSwap(swap, request.ReviewedBy,
                "Ca lam viec da bat dau, khong the duyet doi ca.");
            await _swapRepo.UpdateAsync(swap);
            throw new InvalidOperationException(
                "Ca lam viec da bat dau, khong the duyet doi ca.");
        }

        var rule = await _shiftRuleService.GetEffectiveRuleAsync(shift.StoreId);
        int minHoursBreak = rule?.MinHoursBetweenShifts ?? 8;
        int maxHoursPerWeek = rule?.MaxHoursPerWeek ?? 48;

        // 4. Validate lai cho B (trung ca, nghi giua ca, gio tuan)
        var warnings = await _validationService.ValidateAssignment(
            shift, swap.TargetUserId, minHoursBreak, maxHoursPerWeek, ct);

        // Neu co BLOCKING thi tu dong reject
        var blocking = warnings.FirstOrDefault(w => w.Severity == "BLOCKING");
        if (blocking != null)
        {
            SwapValidationHelper.RejectSwap(swap, request.ReviewedBy,
                $"Nguoi nhan khong du dieu kien: {blocking.Message}");
            await _swapRepo.UpdateAsync(swap);
            throw new InvalidOperationException(
                $"Khong the duyet: {blocking.Message}");
        }

        // 5. Swap: doi UserId tu A sang B
        assignment.UserId = swap.TargetUserId;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;
        await _assignmentRepo.UpdateAsync(assignment);

        // 6. Cap nhat trang thai thanh cong
        swap.Status = (int)SwapRequestStatus.Approved;
        swap.ReviewedBy = request.ReviewedBy;
        swap.ReviewedAt = DateTimeOffset.UtcNow;
        swap.UpdatedAt = DateTimeOffset.UtcNow;
        await _swapRepo.UpdateAsync(swap);

        return true;
    }
}
