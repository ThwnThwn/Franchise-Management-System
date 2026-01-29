using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.Auth.Response
{
    public class ConfirmOtpForgetPasswordOtpRep
    {
        public string AccessForgetPasswordToken { get; set; } = null!;
    }
}
