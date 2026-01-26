using SalesService.Domain.Commons;
using SalesService.Domain.Entities;

namespace SalesService.Domain.Entities;

public partial class OrderItem : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string ProductName { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid ProductId { get; set; }

    public Guid VariantId { get; set; }

    public Guid OrderId { get; set; }

    public virtual Order Order { get; set; } = null!;
}