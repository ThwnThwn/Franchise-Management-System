namespace SupplierService.Domain.Commons;

/// <summary>
/// Base entity bao gồm audit fields và soft-delete, giống pattern SalesService.
/// </summary>
public abstract class BaseEntity : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public abstract class SoftDeletedEntity
{
    public bool IsDeleted { get; set; }
}
