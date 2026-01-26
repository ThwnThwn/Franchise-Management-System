using System;
using System.Collections.Generic;
using SalesService.Domain.Commons;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities;

public partial class Order : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? PaidAmount { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    public OrderStatus Status { get; set; }

    public DateTimeOffset PaidAt { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? CustomerId { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}