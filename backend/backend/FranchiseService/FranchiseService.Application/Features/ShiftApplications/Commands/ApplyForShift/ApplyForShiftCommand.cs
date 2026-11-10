using MediatR;

namespace FranchiseService.Application.Features.ShiftApplications.Commands.ApplyForShift;

/// <summary>
/// Command nhan vien dang ky nhan ca tu Cho Ca.
/// He thong validate ngay: neu vi pham bat ky rule nao thi tu choi luon (khong tao don rac).
/// </summary>
public class ApplyForShiftCommand : IRequest<Guid>
{
    /// <summary>
    /// ID ca lam viec muon dang ky
    /// </summary>
    public Guid ShiftId { get; set; }

    /// <summary>
    /// ID nhan vien dang ky
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Ghi chu cua nhan vien (vi du: "Em ranh ca nay, xin nhan")
    /// </summary>
    public string? Note { get; set; }
}
