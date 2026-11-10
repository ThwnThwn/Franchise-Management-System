namespace SupplierService.Domain.Entities;

public class SupplierIngredient
{
    public Guid SupplierId { get; set; }
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = null!;
    public string IngredientUnit { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int? LeadTimeDays { get; set; }
    public bool IsPrimary { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Supplier Supplier { get; set; } = null!;
}
