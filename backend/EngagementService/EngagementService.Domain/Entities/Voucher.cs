using EngagementService.Domain.Commons;
using EngagementService.Domain.Constants;

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

    public int? ReservedQuantity { get; set; }

    public int? MaxUsagePerUser { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<UserVoucher> UserVouchers { get; set; } = [];

    public virtual ICollection<VoucherItem> VoucherItems { get; set; } = [];

    public virtual ICollection<VoucherQueue> VoucherQueues { get; set; } = [];

    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = [];

    public Guid? CampaignId { get; set; }   
    public Campaign? Campaign { get; set; }
}