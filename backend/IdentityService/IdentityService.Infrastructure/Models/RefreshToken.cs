using System;
using System.Collections.Generic;

namespace IdentityService.Infrastructure.Models;

public partial class RefreshToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime? IssuedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsRevoked { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
