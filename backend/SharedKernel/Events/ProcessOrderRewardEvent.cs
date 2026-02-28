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
        public decimal TotalAmount { get; set; } = 0;
    }
}
