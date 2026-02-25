using EngagementService.Domain.Commons;
using System;
using System.Collections.Generic;

namespace EngagementService.Domain.Entities;

public partial class VoucherQueue : IEntity
{
    public Guid Id { get; set; }

    public Guid VoucherId { get; set; }

    public Guid OrderId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public virtual Voucher Voucher { get; set; } = null!;
}
