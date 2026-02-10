using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

public partial class StoreHour : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public int DayOfWeek { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }

    public bool? IsClosed { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Store Store { get; set; } = null!;
}