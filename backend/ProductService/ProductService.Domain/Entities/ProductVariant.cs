using System;
using System.Collections.Generic;

namespace ProductService.Domain.Entities;

public partial class ProductVariant
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = null!;

    public string? Name { get; set; }

    public string? Attributes { get; set; }

    public decimal BasePrice { get; set; }

    public decimal Cost { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public Guid ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;
}
