using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.RespondSwap;

/// <summary>
/// B tra loi yeu cau doi ca: dong y hoac tu choi.
/// </summary>
public class RespondSwapCommand : IRequest<bool>
{
    /// <summary>
    /// ID yeu cau doi ca
    /// </summary>
    public Guid SwapRequestId { get; set; }

    /// <summary>
    /// ID nguoi tra loi (B, lay tu JWT)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// true = dong y, false = tu choi
    /// </summary>
    public bool Accept { get; set; }

    /// <summary>
    /// Ly do tu choi (bat buoc neu tu choi)
    /// </summary>
    public string? RejectReason { get; set; }
}
