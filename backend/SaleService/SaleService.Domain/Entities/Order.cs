using System;
using System.Collections.Generic;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities;

public partial class Order
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool? IsDeleted { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
