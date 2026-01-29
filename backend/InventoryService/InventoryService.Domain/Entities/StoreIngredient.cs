using InventoryService.Domain.Commons;
using System;
using System.Collections.Generic;

namespace InventoryService.Domain.Entities;

public partial class StoreIngredient : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public Guid IngredientId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? MinThreshold { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Ingredient Ingredient { get; set; } = null!;
}
