using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.User.Request
{
    public class CustomerCreateUserReq
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        [MaxLength(100)]
        public string? Name { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

    }
}
