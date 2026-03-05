using FranchiseService.Application.Features.ShiftSwaps.Helpers;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.RespondSwap;

/// <summary>
/// Xu ly phan hoi cua B cho yeu cau doi ca.
///
/// Validate: yeu cau ton tai, chi B tra loi, dang PendingTarget, chua het han.
/// Accept thi chuyen PendingManager, Reject thi dung RejectSwap helper.
/// </summary>
public class RespondSwapHandler : IRequestHandler<RespondSwapCommand, bool>
{
    private readonly IGenericRepository<ShiftSwapRequest> _swapRepo;

    public RespondSwapHandler(IGenericRepository<ShiftSwapRequest> swapRepo)
    {
        _swapRepo = swapRepo;
    }

    public async Task<bool> Handle(RespondSwapCommand request, CancellationToken ct)
    {
        var swap = await _swapRepo.GetByIdAsync(request.SwapRequestId);
        if (swap == null)
            throw new InvalidOperationException("Yeu cau doi ca khong ton tai.");

        // Chi B moi duoc tra loi
        if (swap.TargetUserId != request.UserId)
            throw new InvalidOperationException(
                "Chi nguoi duoc yeu cau moi co quyen tra loi.");

        // Phai dang cho B xac nhan
        if (swap.Status != (int)SwapRequestStatus.PendingTarget)
            throw new InvalidOperationException(
                "Yeu cau khong con o trang thai cho xac nhan.");

        // Lazy check het han
        if (DateTimeOffset.UtcNow > swap.ExpiresAt)
        {
            swap.Status = (int)SwapRequestStatus.Expired;
            swap.UpdatedAt = DateTimeOffset.UtcNow;
            await _swapRepo.UpdateAsync(swap);
            throw new InvalidOperationException("Yeu cau da het han.");
        }

        // Cap nhat trang thai
        if (request.Accept)
        {
            swap.Status = (int)SwapRequestStatus.PendingManager;
        }
        else
        {
            SwapValidationHelper.RejectSwap(swap, null,
                request.RejectReason ?? "Nguoi nhan tu choi.");
        }

        swap.UpdatedAt = DateTimeOffset.UtcNow;
        await _swapRepo.UpdateAsync(swap);
        return request.Accept;
    }
}
