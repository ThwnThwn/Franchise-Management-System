using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.CheckOut;

/// <summary>
/// Command chấm công tan ca.
/// Cập nhật TimeLog hiện có với thông tin check-out.
/// </summary>
public class CheckOutCommand : IRequest<bool>
{
    public Guid LogId { get; set; }
    public Guid UserId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Note { get; set; }
}
