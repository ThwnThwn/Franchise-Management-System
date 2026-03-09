using FranchiseService.Application.Features.ShiftSwaps.Helpers;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.ReleaseShift;

/// <summary>
/// Xu ly nhan vien nha ca ve Cho Ca.
/// Dung SwapValidationHelper cho check chung.
/// Sau do kiem tra them: khong co swap pending tren ca nay.
/// </summary>
public class ReleaseShiftHandler : IRequestHandler<ReleaseShiftCommand, bool>
{
    private readonly SwapValidationHelper _helper;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepo;
    private readonly IGenericRepository<Shift> _shiftRepo;
    private readonly IGenericRepository<ShiftSwapRequest> _swapRepo;

    public ReleaseShiftHandler(
        SwapValidationHelper helper,
        IGenericRepository<ShiftAssignment> assignmentRepo,
        IGenericRepository<Shift> shiftRepo,
        IGenericRepository<ShiftSwapRequest> swapRepo)
    {
        _helper = helper;
        _assignmentRepo = assignmentRepo;
        _shiftRepo = shiftRepo;
        _swapRepo = swapRepo;
    }

    public async Task<bool> Handle(ReleaseShiftCommand request, CancellationToken ct)
    {
        // 1. Kiem tra dieu kien chung (assignment, Published, AllowShiftSwap)
        var check = await _helper.ValidatePreconditionsAsync(
            request.AssignmentId, request.UserId);

        // 2. Khong co swap pending tren assignment nay
        var pendingSwaps = await _swapRepo.GetAllAsync(
            predicate: s => s.SourceAssignmentId == check.Assignment.Id
                && (s.Status == (int)SwapRequestStatus.PendingTarget
                    || s.Status == (int)SwapRequestStatus.PendingManager));
        if (pendingSwaps.Any())
            throw new InvalidOperationException(
                "Dang co yeu cau doi ca cho xu ly. Hay huy truoc khi nha ca.");

        // 3. Soft-delete assignment
        await _assignmentRepo.DeleteAsync(check.Assignment.Id);

        // 4. Neu thieu nguoi: chuyen ca sang Open (Cho Ca)
        var remaining = await _assignmentRepo.GetAllAsync(
            predicate: a => a.ShiftId == check.Shift.Id
                && a.Id != check.Assignment.Id);

        if (remaining.Count() < check.Shift.RequiredStaffCount)
        {
            check.Shift.Status = (int)ShiftStatus.Open;
            check.Shift.UpdatedAt = DateTimeOffset.UtcNow;
            await _shiftRepo.UpdateAsync(check.Shift);
        }

        return true;
    }
}
