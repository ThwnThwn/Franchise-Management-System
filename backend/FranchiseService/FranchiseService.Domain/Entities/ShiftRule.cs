using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

public partial class ShiftRule : IEntity
{
    public Guid Id { get; set; }

    public Guid? StoreId { get; set; }

    public bool? AllowShuttle { get; set; }

    public bool? AllowShiftSwap { get; set; }

    public int? MinHoursBetweenShifts { get; set; }

    public int? MaxHoursPerWeek { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}