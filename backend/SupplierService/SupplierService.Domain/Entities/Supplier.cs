using SupplierService.Domain.Commons;

namespace SupplierService.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<SupplierIngredient> SupplierIngredients { get; set; } = new List<SupplierIngredient>();
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
