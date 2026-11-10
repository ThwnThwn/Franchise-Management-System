using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

public partial class Store : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? ImageUrl { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int Type { get; set; }

    public int Status { get; set; }

    public Guid? FranchiseeId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Franchisee? Franchisee { get; set; }

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    public virtual ICollection<StoreHour> StoreHours { get; set; } = new List<StoreHour>();
}