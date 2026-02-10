using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

public partial class ShiftAssignment : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid ShiftId { get; set; }

    public Guid UserId { get; set; }

    public int? Status { get; set; }

    public decimal? HourlyWage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Shift Shift { get; set; } = null!;

    public virtual ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
}