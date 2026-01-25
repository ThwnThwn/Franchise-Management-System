using IdentityService.Application.Dtos.Role.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.User.Respone
{
    public class UserRep
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;

        public string? Username { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public bool? IsBlocked { get; set; }

        public RoleUserRep? Role { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
