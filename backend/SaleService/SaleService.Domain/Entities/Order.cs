using SalesService.Domain.Commons;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities;

public partial class Order : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Tax { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? PaidAmount { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    public OrderStatus Status { get; set; }

    public DateTimeOffset PaidAt { get; set; }

    public string? Notes { get; set; }
    public OrderType? Type { get; set; }

    public TimeOnly? ScheduledTime { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public decimal? Distance { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? StoreId { get; set; }

    public Guid? DelivererId { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderSaleItem> OrderSaleItems { get; set; } = new List<OrderSaleItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

    public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();
}
