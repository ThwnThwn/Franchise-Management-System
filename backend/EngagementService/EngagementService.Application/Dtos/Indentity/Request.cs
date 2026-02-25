using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.Indentity
{
    public class GetUserIdByEmailRequest
    {
        public string Email { get; set; } = "";
    }

    public class GetUserIdByPhoneRequest
    {
        public string PhoneNumber { get; set; } = "";
    }
}
