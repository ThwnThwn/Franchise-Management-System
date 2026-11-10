using EngagementService.Domain.Commons;
using System;
using System.Collections.Generic;

namespace EngagementService.Domain.Entities;

public partial class VoucherUsage : IEntity
{
    public Guid Id { get; set; }

    public Guid UserVoucherId { get; set; }

    public Guid VoucherId { get; set; }

    public Guid OrderId { get; set; }

    public DateTimeOffset? AppliedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual UserVoucher? UserVoucher { get; set; }

    public virtual Voucher Voucher { get; set; } = null!;
}
