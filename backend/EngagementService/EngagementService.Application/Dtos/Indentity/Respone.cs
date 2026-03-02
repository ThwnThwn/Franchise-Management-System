using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.Indentity
{
    public class UserSimpleResp
    {
        public List<UserIDNameResp> UserIDNameResp { get; set; } = new List<UserIDNameResp>();

    }   

    public class UserIDNameResp
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
