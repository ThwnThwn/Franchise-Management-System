using MediatR;
using FranchiseService.Application.Features.ShiftRules.DTOs;

namespace FranchiseService.Application.Features.ShiftRules.Commands.SaveShiftRule;

public class SaveShiftRuleCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public SaveShiftRuleRequest Request { get; set; }

    public SaveShiftRuleCommand(Guid userId, SaveShiftRuleRequest request)
    {
        UserId = userId;
        Request = request;
    }
}
