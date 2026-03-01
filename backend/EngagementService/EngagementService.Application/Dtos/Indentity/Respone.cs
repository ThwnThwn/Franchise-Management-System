using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.Indentity
{
    public class UserIdsResp
    {
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }   
}
