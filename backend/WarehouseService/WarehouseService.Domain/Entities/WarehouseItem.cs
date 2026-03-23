using WarehouseService.Domain.Commons;

namespace WarehouseService.Domain.Entities;

public partial class WarehouseItem : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid IngredientId { get; set; }
    public string Unit { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal BasePrice { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}