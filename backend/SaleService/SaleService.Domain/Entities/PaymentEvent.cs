using System;
using System.Collections.Generic;

namespace SalesService.Domain.Entities;
public partial class PaymentEvent
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    public string EventType { get; set; } = null!;

    public string? Payload { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
