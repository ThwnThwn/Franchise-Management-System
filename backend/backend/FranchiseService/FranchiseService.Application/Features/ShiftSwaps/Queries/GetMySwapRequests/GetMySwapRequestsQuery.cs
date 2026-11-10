using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Queries.GetMySwapRequests;

/// <summary>
/// Lay danh sach yeu cau doi ca lien quan den toi
/// (ca gui di va nhan duoc).
/// </summary>
public class GetMySwapRequestsQuery : IRequest<List<SwapRequestDto>>
{
    public Guid UserId { get; set; }
}

/// <summary>
/// Thong tin yeu cau doi ca tra ve cho FE.
/// </summary>
public class SwapRequestDto
{
    public Guid Id { get; set; }
    public Guid SourceUserId { get; set; }
    public string? SourceUserName { get; set; }
    public Guid TargetUserId { get; set; }
    public string? TargetUserName { get; set; }
    public int Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Thong tin ca
    public Guid ShiftId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
