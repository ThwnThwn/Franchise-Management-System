namespace FranchiseService.Application.Features.ShiftRules.DTOs;

public class ShiftRuleSettingsResponse
{
    public ShiftRuleResponse? Global { get; set; }
    public List<ShiftRuleResponse>? Stores { get; set; }
}
