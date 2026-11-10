using ProductService.Domain.Commons;

namespace ProductService.Domain.Entities;

public partial class ProductVariant : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = null!;

    public string? Name { get; set; }

    public string? Attributes { get; set; }

    public decimal BasePrice { get; set; }

    public decimal Cost { get; set; }

    public bool? IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;
}