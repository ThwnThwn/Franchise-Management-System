using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.Auth.Request
{
    public class ForgetPasswordReq
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

    }
}
