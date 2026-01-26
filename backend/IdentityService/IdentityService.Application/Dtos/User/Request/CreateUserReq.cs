using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.User.Request
{
    public class CreateUserReq
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [MaxLength(50)]
        public string? LastName { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        [MaxLength(100)]
        public string? Username { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [Required]
        [Range(0, 1, ErrorMessage = "Gender must be 0 or 1.")]
        public int Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public Guid? RoleId { get; set; }
    }
}
