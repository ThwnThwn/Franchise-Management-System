using WarehouseService.Domain.Commons;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Domain.Entities;

public partial class Warehouse : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public Guid StoreId { get; set; }
    public WarehouseStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<WarehouseItem> WarehouseItems { get; set; } = new List<WarehouseItem>();
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    public virtual ICollection<WarehouseTransfer> TransfersOut { get; set; } = new List<WarehouseTransfer>();
    public virtual ICollection<WarehouseTransfer> TransfersIn { get; set; } = new List<WarehouseTransfer>();
}