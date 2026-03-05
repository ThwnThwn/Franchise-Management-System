using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.CancelSwap;

/// <summary>
/// A tu huy yeu cau doi ca (chi khi dang Pending).
/// </summary>
public class CancelSwapCommand : IRequest<bool>
{
    public Guid SwapRequestId { get; set; }

    /// <summary>
    /// ID nguoi huy (A, lay tu JWT)
    /// </summary>
    public Guid UserId { get; set; }
}
