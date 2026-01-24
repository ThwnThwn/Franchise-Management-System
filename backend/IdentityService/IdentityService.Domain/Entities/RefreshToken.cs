using IdentityService.Domain.Commons;
using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class RefreshToken : IEntity
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTimeOffset? IssuedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public bool? IsRevoked { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
