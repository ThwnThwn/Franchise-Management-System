using IdentityService.Domain.Commons;
using System;
using System.Collections.Generic;
namespace IdentityService.Domain.Entities;

public partial class User : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Username { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public bool? IsBlocked { get; set; }

    public Guid? RoleId { get; set; }
    public virtual Role? Role { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

}
