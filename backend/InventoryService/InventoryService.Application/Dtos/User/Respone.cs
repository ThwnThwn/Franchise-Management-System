using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Dtos.User
{
    public class UserRep
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string? LastName { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public int Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? IsBlocked { get; set; }

        public RoleUserRep? Role { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class RoleUserRep
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
    }
}
