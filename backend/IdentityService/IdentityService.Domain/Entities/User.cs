using IdentityService.Domain.Commons;

namespace IdentityService.Domain.Entities;

public partial class User : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public int Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public bool? IsBlocked { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? RoleId { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual Role? Role { get; set; }
}