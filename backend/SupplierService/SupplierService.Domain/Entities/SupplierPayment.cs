using SupplierService.Domain.Commons;

namespace SupplierService.Domain.Entities;

public class SupplierPayment : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }

    public string Code { get; set; } = null!;

    public decimal Amount { get; set; }

    /// <summary>0=Cash, 1=VNPay</summary>
    public int Method { get; set; }

    /// <summary>0=Pending, 1=Paid, 2=Failed</summary>
    public int Status { get; set; }

    public string? Provider { get; set; }

    public string? ProviderTxnId { get; set; }

    public string? ProviderPayload { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}
