using MediatR;
using FranchiseService.Application.Features.ShiftRules.DTOs;

namespace FranchiseService.Application.Features.ShiftRules.Queries.GetShiftRuleSettings;

public class GetShiftRuleSettingsQuery : IRequest<ShiftRuleSettingsResponse>
{
    public Guid? StoreId { get; set; }
}
