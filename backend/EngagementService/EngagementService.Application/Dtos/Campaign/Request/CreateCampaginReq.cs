using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.Campaign.Request
{
    public class CreateCampaginReq
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public IEnumerable<Guid> VoucherIds { get; set; } = [];
    }
}
