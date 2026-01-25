using System;

namespace SalesService.Domain.Entities;

public partial class OrderItem
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string ProductName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool? IsDeleted { get; set; }
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
}
