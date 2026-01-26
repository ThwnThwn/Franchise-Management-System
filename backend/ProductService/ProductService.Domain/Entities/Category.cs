using System;
using System.Collections.Generic;
using ProductService.Domain.Commons;

namespace ProductService.Domain.Entities;

public partial class Category : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}