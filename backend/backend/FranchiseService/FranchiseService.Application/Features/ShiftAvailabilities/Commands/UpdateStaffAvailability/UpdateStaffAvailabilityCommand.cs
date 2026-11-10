using FranchiseService.Application.Features.ShiftAvailabilities.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Commands.UpdateStaffAvailability;

public class UpdateStaffAvailabilityCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // De check quyen so huu
    public UpdateStaffAvailabilityRequest Request { get; set; }

    public UpdateStaffAvailabilityCommand(Guid id, Guid userId, UpdateStaffAvailabilityRequest request)
    {
        Id = id;
        UserId = userId;
        Request = request;
    }
}
