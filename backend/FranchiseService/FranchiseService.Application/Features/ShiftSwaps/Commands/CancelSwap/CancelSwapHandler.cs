using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.CancelSwap;

/// <summary>
/// A tu huy yeu cau doi ca. Chi A huy duoc, va chi khi dang Pending.
/// </summary>
public class CancelSwapHandler : IRequestHandler<CancelSwapCommand, bool>
{
    private readonly IGenericRepository<ShiftSwapRequest> _swapRepo;

    public CancelSwapHandler(IGenericRepository<ShiftSwapRequest> swapRepo)
    {
        _swapRepo = swapRepo;
    }

    public async Task<bool> Handle(CancelSwapCommand request, CancellationToken ct)
    {
        var swap = await _swapRepo.GetByIdAsync(request.SwapRequestId);
        if (swap == null)
            throw new InvalidOperationException("Yeu cau doi ca khong ton tai.");

        // Chi A moi duoc huy
        if (swap.SourceUserId != request.UserId)
            throw new InvalidOperationException(
                "Chi nguoi gui yeu cau moi co quyen huy.");

        // Chi huy duoc khi dang Pending
        if (swap.Status != (int)SwapRequestStatus.PendingTarget
            && swap.Status != (int)SwapRequestStatus.PendingManager)
            throw new InvalidOperationException(
                "Yeu cau da duoc xu ly, khong the huy.");

        swap.Status = (int)SwapRequestStatus.Cancelled;
        swap.UpdatedAt = DateTimeOffset.UtcNow;
        await _swapRepo.UpdateAsync(swap);

        return true;
    }
}
