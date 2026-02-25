using MediatR;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Commands.DeleteStaffAvailability;

public record DeleteStaffAvailabilityCommand(Guid Id, Guid UserId) : IRequest<Unit>;
