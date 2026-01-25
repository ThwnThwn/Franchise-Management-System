using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.Role.Request
{
    public class CreateRoleReq
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Slug { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
