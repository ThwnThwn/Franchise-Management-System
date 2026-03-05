using FranchiseService.Application.Features.ShiftApplications.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.ShiftApplications.Queries.GetApplicationsByShift;

/// <summary>
/// Query lay danh sach don dang ky cua 1 ca kem Smart Score.
/// Dung cho Manager xem va chon ung vien phu hop.
/// </summary>
public class GetApplicationsByShiftQuery : IRequest<List<ShiftApplicationDto>>
{
    /// <summary>
    /// ID ca lam viec can xem danh sach ung vien
    /// </summary>
    public Guid ShiftId { get; set; }
}
