using FranchiseService.Application.Features.ShiftAvailabilities.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Commands.CreateStaffAvailability;

public class CreateStaffAvailabilityCommand : IRequest<Guid>
{
    // User ID duoc lay tu token/header (x-user-id)
    public Guid UserId { get; set; }

    public CreateStaffAvailabilityRequest Request { get; set; }

    public CreateStaffAvailabilityCommand(Guid userId, CreateStaffAvailabilityRequest request)
    {
        UserId = userId;
        Request = request;
    }
}
