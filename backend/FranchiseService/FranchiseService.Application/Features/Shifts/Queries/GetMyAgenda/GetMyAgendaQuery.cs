using FranchiseService.Application.Features.Shifts.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetMyAgenda;

/// <summary>
/// Query lay lich ca nhan cua nhan vien theo khung ngay
/// </summary>
public class GetMyAgendaQuery : IRequest<List<StaffShiftAgendaDto>>
{
    public Guid UserId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
