using System;
using System.Collections.Generic;

namespace IdentityService.Infrastructure.Models;

public partial class Permission
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string Resource { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
