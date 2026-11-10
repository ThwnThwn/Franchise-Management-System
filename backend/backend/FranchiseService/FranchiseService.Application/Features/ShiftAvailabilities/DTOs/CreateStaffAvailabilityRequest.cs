namespace FranchiseService.Application.Features.ShiftAvailabilities.DTOs;

public class CreateStaffAvailabilityRequest
{
    public bool IsAvailable { get; set; }
    public bool IsRecurring { get; set; }
    public DateOnly? SpecificDate { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Note { get; set; }
}
