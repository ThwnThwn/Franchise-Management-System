namespace FranchiseService.Application.Features.ShiftRules.DTOs;

public class ShiftRuleResponse
{
    public Guid Id { get; set; }
    public Guid? StoreId { get; set; }
    public bool AllowShuttle { get; set; }
    public bool AllowShiftSwap { get; set; }
    public int MinHoursBetweenShifts { get; set; }
    public int MaxHoursPerWeek { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
