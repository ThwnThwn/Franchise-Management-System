using EngagementService.Domain.Commons;
using EngagementService.Domain.Constants;
using System;
using System.Collections.Generic;

namespace EngagementService.Domain.Entities;

public partial class PointLog : IEntity
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public decimal Amount { get; set; }

    public decimal? BalanceBefore { get; set; }

    public decimal? BalanceAfter { get; set; }

    public PointActionType ActionType { get; set; }

    public string? ReasonCode { get; set; }

    public Guid? ReferenceId { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual LoyaltyAccount? User { get; set; }
}
