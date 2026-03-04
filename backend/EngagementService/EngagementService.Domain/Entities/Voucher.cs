using EngagementService.Domain.Commons;
using EngagementService.Domain.Constants;
using System;
using System.Collections.Generic;

namespace EngagementService.Domain.Entities;

public partial class Voucher : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public VoucherType Type { get; set; }

    public decimal? DiscountValue { get; set; }

    public decimal? MaxDiscountValue { get; set; }

    public decimal? MinOrderValue { get; set; }

    public decimal? PointCost { get; set; }

    public int? InitQuantity { get; set; }

    public int? CurrentQuantity { get; set; }

    public int? MaxUsagePerUser { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();

    public virtual ICollection<VoucherItem> VoucherItems { get; set; } = new List<VoucherItem>();

    public virtual ICollection<VoucherQueue> VoucherQueues { get; set; } = new List<VoucherQueue>();

    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}
