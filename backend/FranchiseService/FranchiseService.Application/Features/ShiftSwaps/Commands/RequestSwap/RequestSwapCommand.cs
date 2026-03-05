using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.RequestSwap;

/// <summary>
/// Lenh gui yeu cau doi ca.
/// A chi dinh ca muon doi va nguoi nhan B.
/// </summary>
public class RequestSwapCommand : IRequest<RequestSwapResult>
{
    /// <summary>
    /// ID phan cong ca ma A muon doi
    /// </summary>
    public Guid AssignmentId { get; set; }

    /// <summary>
    /// ID nguoi gui (A, lay tu JWT)
    /// </summary>
    public Guid SourceUserId { get; set; }

    /// <summary>
    /// ID nguoi nhan ca (B)
    /// </summary>
    public Guid TargetUserId { get; set; }

    /// <summary>
    /// Ly do doi ca (bat buoc)
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Ket qua sau khi tao yeu cau doi ca thanh cong.
/// </summary>
public class RequestSwapResult
{
    public Guid SwapRequestId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
