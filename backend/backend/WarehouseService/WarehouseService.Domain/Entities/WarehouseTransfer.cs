using WarehouseService.Domain.Commons;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Domain.Entities;

public partial class WarehouseTransfer : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }
    public string TransferCode { get; set; } = null!;
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public TransferStatus Status { get; set; }
    public string? Reason { get; set; }
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Warehouse FromWarehouse { get; set; } = null!;
    public virtual Warehouse ToWarehouse { get; set; } = null!;
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    public virtual ICollection<WarehouseTransferItem> TransferItems { get; set; } = new List<WarehouseTransferItem>();
}