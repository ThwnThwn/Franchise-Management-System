using WarehouseService.Domain.Commons;

namespace WarehouseService.Domain.Entities;

public partial class WarehouseTransferItem : IEntity
{
    public Guid Id { get; set; }

    public Guid WarehouseTransferId { get; set; }
    public Guid WarehouseItemId { get; set; }   
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }        
    public decimal UnitPrice { get; set; }       

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public virtual WarehouseTransfer WarehouseTransfer { get; set; } = null!;
    public virtual WarehouseItem WarehouseItem { get; set; } = null!;
}