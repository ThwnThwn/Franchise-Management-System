using InventoryService.Domain.Commons;
using System;
using System.Collections.Generic;

namespace InventoryService.Domain.Entities;

public partial class Ingredient : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string BaseUnit { get; set; } = null!;

    public decimal Cost { get; set; }

    public Guid? CategoryId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual IngredientCategory? IngredientCategory { get; set; }

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public virtual ICollection<StoreIngredient> StoreIngredients { get; set; } = new List<StoreIngredient>();
}
