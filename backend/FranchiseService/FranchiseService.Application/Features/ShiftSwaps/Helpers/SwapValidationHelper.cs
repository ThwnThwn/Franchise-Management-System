using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Features.ShiftSwaps.Helpers;

/// <summary>
/// Ket qua sau khi kiem tra dieu kien chung cho doi ca hoac nha ca.
/// Gom Assignment, Shift va Rule de Handler dung lai ma khong can query DB lan nua.
/// </summary>
public record SwapPreCheck(ShiftAssignment Assignment, Shift Shift, ShiftRule? Rule);

/// <summary>
/// Gom logic validate chung cho RequestSwap va ReleaseShift.
/// Tranh duplicate code giua 2 handler.
///
/// Kiem tra:
///   1. Assignment ton tai va thuoc ve nguoi gui
///   2. Ca phai Published va chua dien ra
///   3. ShiftRule cho phep doi ca (AllowShiftSwap)
/// </summary>
public class SwapValidationHelper
{
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepo;
    private readonly IGenericRepository<Shift> _shiftRepo;
    private readonly IShiftRuleService _ruleService;

    public SwapValidationHelper(
        IGenericRepository<ShiftAssignment> assignmentRepo,
        IGenericRepository<Shift> shiftRepo,
        IShiftRuleService ruleService)
    {
        _assignmentRepo = assignmentRepo;
        _shiftRepo = shiftRepo;
        _ruleService = ruleService;
    }

    /// <summary>
    /// Kiem tra dieu kien bat buoc truoc khi doi ca hoac nha ca.
    /// Neu khong hop le se throw InvalidOperationException.
    /// </summary>
    public async Task<SwapPreCheck> ValidatePreconditionsAsync(
        Guid assignmentId, Guid userId)
    {
        // 1. Assignment ton tai
        var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
        if (assignment == null)
            throw new InvalidOperationException("Phan cong khong ton tai.");

        // 2. Thuoc ve nguoi gui
        if (assignment.UserId != userId)
            throw new InvalidOperationException(
                "Ban khong duoc phan cong ca nay.");

        // 3. Ca phai Published
        var shift = await _shiftRepo.GetByIdAsync(assignment.ShiftId);
        if (shift == null)
            throw new InvalidOperationException("Ca lam viec khong ton tai.");
        if (shift.Status != (int)ShiftStatus.Published)
            throw new InvalidOperationException(
                "Chi duoc thao tac tren ca da Published.");

        // 4. Ca chua dien ra (so sanh ca ngay va gio bat dau)
        var hienTaiVn = DateTime.UtcNow.AddHours(7);
        var thoiDiemBatDauCa = shift.ShiftDate.ToDateTime(shift.StartTime);
        if (hienTaiVn >= thoiDiemBatDauCa)
            throw new InvalidOperationException(
                "Chi duoc thao tac tren ca chua dien ra.");

        // 5. ShiftRule cho phep
        var rule = await _ruleService.GetEffectiveRuleAsync(shift.StoreId);
        if (rule != null && !rule.AllowShiftSwap)
            throw new InvalidOperationException(
                "Cua hang khong cho phep doi ca hoac nha ca.");

        return new SwapPreCheck(assignment, shift, rule);
    }

    /// <summary>
    /// Tu choi yeu cau doi ca va cap nhat ly do.
    /// Dung chung cho ApproveSwap khi Manager reject, B vi pham, assignment bi xoa.
    /// </summary>
    public static void RejectSwap(
        ShiftSwapRequest swap, Guid? reviewedBy, string reason)
    {
        swap.Status = (int)SwapRequestStatus.Rejected;
        swap.RejectReason = reason;
        swap.ReviewedBy = reviewedBy;
        swap.ReviewedAt = DateTimeOffset.UtcNow;
        swap.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
