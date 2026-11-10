using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.User.Request
{
    public class UpdateUserReq
    {
        [MaxLength(100)]
        public string? Username { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public string? Password { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [Range(0, 1, ErrorMessage = "Gender must be 0 or 1.")]
        public int? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? IsBlocked { get; set; }

        [Required]
        public string RoleSlug { get; set; } = null!;
        public Guid? HomeStoreId { get; set; }
        public decimal? CurrentHourlyWage { get; set; }
    }
}