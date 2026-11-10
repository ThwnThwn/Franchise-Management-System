using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.DeleteShift
{
    public record DeleteShiftCommand(Guid Id) : IRequest<Unit>;
}
