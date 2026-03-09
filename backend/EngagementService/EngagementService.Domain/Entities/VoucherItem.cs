using EngagementService.Domain.Commons;
using EngagementService.Domain.Constants;
using System;
using System.Collections.Generic;

namespace EngagementService.Domain.Entities;

public partial class VoucherItem : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid VoucherId { get; set; }

    public string Sku { get; set; }

    public int Quantity { get; set; }

    public VoucherItemType Type { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Voucher Voucher { get; set; } = null!;
}
