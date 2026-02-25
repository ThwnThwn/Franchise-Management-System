using EngagementService.Domain.Commons;
using System;
using System.Collections.Generic;

namespace EngagementService.Domain.Entities;

public partial class MembershipTier : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal MinSpendRequired { get; set; }

    public double DiscountRate { get; set; }

    public decimal AmountPerPoint { get; set; } // đổi từ tiền sang điểm

    public decimal ValuePerPoint { get; set; }  // đổi từ điểm sang tiền

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<LoyaltyAccount> LoyaltyAccounts { get; set; } = new List<LoyaltyAccount>();
}
