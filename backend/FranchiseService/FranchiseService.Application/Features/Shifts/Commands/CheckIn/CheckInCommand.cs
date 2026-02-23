using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.CheckIn;

/// <summary>
/// Command chấm công vào ca.
/// Tạo bản ghi TimeLog mới với trạng thái Processing.
/// </summary>
public class CheckInCommand : IRequest<Guid>
{
    public Guid AssignmentId { get; set; }
    public Guid UserId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Note { get; set; }
}
