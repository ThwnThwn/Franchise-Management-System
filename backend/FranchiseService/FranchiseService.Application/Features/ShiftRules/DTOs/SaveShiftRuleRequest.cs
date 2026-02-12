namespace FranchiseService.Application.Features.ShiftRules.DTOs;

public class SaveShiftRuleRequest
{
    /// <summary>
    /// ID cua cua hang (Opsional).
    /// Neu null -> Update Global Rule (Chi Admin).
    /// Neu co gia tri -> Update Store Rule.
    /// </summary>
    public Guid? StoreId { get; set; }

    public bool AllowShuttle { get; set; }
    public bool AllowShiftSwap { get; set; }
    public int MinHoursBetweenShifts { get; set; }
    public int MaxHoursPerWeek { get; set; }
}
