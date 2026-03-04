using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Events
{
    public class ProcessOrderRewardEvent
    {
        public Guid Id { get; set; }
        public decimal OrderAmount { get; set; } = 0;
        public Guid OrderId { get; set; } = Guid.Empty;
    }
}
