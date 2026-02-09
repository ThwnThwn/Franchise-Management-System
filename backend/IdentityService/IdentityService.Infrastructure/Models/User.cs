using System;
using System.Collections.Generic;

namespace IdentityService.Infrastructure.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string? Username { get; set; }

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public int Gender { get; set; }

    public DateTime DateOfBirth { get; set; }

    public bool? IsBlocked { get; set; }

    public Guid? HomeStoreId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public Guid? RoleId { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual Role? Role { get; set; }
}
