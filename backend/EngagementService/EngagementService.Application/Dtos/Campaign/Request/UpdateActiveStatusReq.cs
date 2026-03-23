using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.Campaign.Request
{
    public class UpdateActiveStatusReq
    {
        public Guid Id {  get; set; }
        public bool IsActive { get; set; }
    }
}
