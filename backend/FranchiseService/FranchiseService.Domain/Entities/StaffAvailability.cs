using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

public partial class StaffAvailability : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateOnly AvailabilityDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool? IsAvailable { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}