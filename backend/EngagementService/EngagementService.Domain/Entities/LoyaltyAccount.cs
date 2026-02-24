using EngagementService.Domain.Commons;
using System;
using System.Collections.Generic;

namespace EngagementService.Domain.Entities;

public partial class LoyaltyAccount : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public decimal AvailablePoints { get; set; }

    public decimal TotalSpent { get; set; }

    public Guid CurrentTierId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual MembershipTier CurrentTier { get; set; } = null!;

    public virtual ICollection<PointLog> PointLogs { get; set; } = new List<PointLog>();

    public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
}
