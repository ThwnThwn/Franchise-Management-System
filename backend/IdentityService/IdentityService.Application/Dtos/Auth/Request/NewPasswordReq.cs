using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.Auth.Request
{
    public class NewPasswordReq
    {
        [Required]
        [MinLength(5)]
        public string NewPassword { get; set; } = null!;

    }
}
