using System;
using System.Collections.Generic;
using SalesService.Domain.Entities;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public string Code { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public int Method { get; set; }

    public int Status { get; set; }

    public string? Provider { get; set; }

    public string? ProviderTxnId { get; set; }

    public string? ProviderPayload { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<PaymentEvent> PaymentEvents { get; set; } = new List<PaymentEvent>();
}
