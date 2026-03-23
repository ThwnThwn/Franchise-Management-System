using WarehouseService.Domain.Commons;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Domain.Entities;

public partial class StockTransaction : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }
    public string TransactionCode { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public Guid WarehouseItemId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Nhập kho từ Supplier
    public Guid? SupplierId { get; set; }
    public string? ReferenceCode { get; set; }

    // Chuyển kho
    public Guid? WarehouseTransferId { get; set; }

    public string? Note { get; set; }
    public TransactionStatus Status { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual WarehouseItem WarehouseItem { get; set; } = null!;
    public virtual WarehouseTransfer? WarehouseTransfer { get; set; }
}