using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.PublishShift
{
    /// <summary>
    /// Command để chuyển trạng thái ca từ Draft → Published.
    /// FE: POST /shifts/{id}/publish
    /// </summary>
    public record PublishShiftCommand(Guid ShiftId) : IRequest<Unit>;
}
