using EngagementService.Domain.Commons;
using EngagementService.Domain.Constants;
using System;
using System.Collections.Generic;

namespace EngagementService.Domain.Entities;

public partial class UserVoucher : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid VoucherId { get; set; }

    public UserVoucherStatus? Status { get; set; }

    public DateTimeOffset? AcquiredAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual LoyaltyAccount User { get; set; } = null!;

    public virtual Voucher Voucher { get; set; } = null!;

    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}
