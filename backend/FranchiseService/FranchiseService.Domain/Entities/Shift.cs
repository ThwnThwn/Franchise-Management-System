using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

public partial class Shift : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public DateOnly ShiftDate { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int RequiredStaffCount { get; set; }

    public Guid? ShiftTemplateId { get; set; }

    public int? Status { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();

    public virtual Store Store { get; set; } = null!;
}