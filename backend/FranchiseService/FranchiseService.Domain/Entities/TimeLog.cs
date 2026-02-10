using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

public partial class TimeLog : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid ShiftAssignmentId { get; set; }

    public DateTime? CheckInAt { get; set; }

    public string? CheckInImageUrl { get; set; }

    public decimal? CheckInLat { get; set; }

    public decimal? CheckInLong { get; set; }

    public DateTime? CheckOutAt { get; set; }

    public string? CheckOutImageUrl { get; set; }

    public decimal? CheckOutLat { get; set; }

    public decimal? CheckOutLong { get; set; }

    public int? TotalWorkMinutes { get; set; }

    public int? Status { get; set; }

    public string? ManagerNote { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ShiftAssignment ShiftAssignment { get; set; } = null!;
}