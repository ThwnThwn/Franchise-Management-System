using SupplierService.Domain.Commons;
using SupplierService.Domain.Enums;

namespace SupplierService.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string Code { get; set; } = null!;
    public Guid SupplierId { get; set; }
    public Guid? StoreId { get; set; }

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }

    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpectedDeliveryDate { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public string? Notes { get; set; }
    public string? ReferenceCode { get; set; }

    public virtual Supplier Supplier { get; set; } = null!;
    public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
