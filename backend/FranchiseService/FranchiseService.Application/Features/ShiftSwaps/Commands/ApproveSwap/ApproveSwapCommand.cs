using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.ApproveSwap;

/// <summary>
/// Manager duyet hoac tu choi yeu cau doi ca.
/// </summary>
public class ApproveSwapCommand : IRequest<bool>
{
    public Guid SwapRequestId { get; set; }

    /// <summary>
    /// ID Manager (lay tu JWT)
    /// </summary>
    public Guid ReviewedBy { get; set; }

    /// <summary>
    /// true = duyet, false = tu choi
    /// </summary>
    public bool Approve { get; set; }

    /// <summary>
    /// Ly do tu choi (bat buoc neu tu choi)
    /// </summary>
    public string? RejectReason { get; set; }
}
