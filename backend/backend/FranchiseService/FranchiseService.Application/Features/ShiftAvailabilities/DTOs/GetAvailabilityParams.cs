namespace FranchiseService.Application.Features.ShiftAvailabilities.DTOs;

public class GetAvailabilityParams
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }

    public bool? IsAvailable { get; set; }
    public bool? IsRecurring { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }

    public Guid? StoreId { get; set; }
    public Guid? UserId { get; set; }

    public string? Keyword { get; set; }
}