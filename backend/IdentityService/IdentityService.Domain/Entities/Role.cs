using IdentityService.Domain.Commons;
using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class Role : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
