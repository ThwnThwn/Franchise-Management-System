using System;
using System.Collections.Generic;
using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Domain.Entities;

public partial class Schedule : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid StoreId { get; set; }

    public Guid? ShiftTemplateId { get; set; }

    public DateOnly ShiftDate { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int? BreakDurationMinutes { get; set; }

    public decimal? HourlyWage { get; set; }

    public int? Status { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Store Store { get; set; } = null!;

    public virtual ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
}