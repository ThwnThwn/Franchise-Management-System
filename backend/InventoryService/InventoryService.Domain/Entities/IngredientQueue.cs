using InventoryService.Domain.Commons;

namespace InventoryService.Domain.Entities;

public class IngredientQueue : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public Guid IngredientId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
