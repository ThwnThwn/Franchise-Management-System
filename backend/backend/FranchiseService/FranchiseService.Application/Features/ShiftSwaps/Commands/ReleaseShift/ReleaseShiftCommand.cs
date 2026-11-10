using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.ReleaseShift;

/// <summary>
/// Nhan vien nha ca ve Cho Ca.
/// Ca chuyen sang Open de nguoi khac dang ky.
/// </summary>
public class ReleaseShiftCommand : IRequest<bool>
{
    /// <summary>
    /// ID phan cong ca muon nha
    /// </summary>
    public Guid AssignmentId { get; set; }

    /// <summary>
    /// ID nguoi nha (lay tu JWT)
    /// </summary>
    public Guid UserId { get; set; }
}
