using System;
using System.Collections.Generic;
using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

public partial class ShiftTemplate : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid? StoreId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int? BreakDurationMinutes { get; set; }

    public bool? IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}