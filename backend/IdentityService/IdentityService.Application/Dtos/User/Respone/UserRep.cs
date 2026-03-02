using IdentityService.Application.Dtos.Loyalty.Response;
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
        public string Username { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool? IsBlocked { get; set; }
        public Guid? HomeStoreId { get; set; }
        public decimal? CurrentHourlyWage { get; set; }
        public RoleUserRep? Role { get; set; }
        public string? AvatarUrl { get; set; }
        public LoyaltyRes? LoyaltyTier { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

    }
}