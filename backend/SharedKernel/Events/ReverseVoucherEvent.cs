using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Events
{
    public class ReverseVoucherEvent
    {
        public Guid OrderId { get; set; }
    }
}
