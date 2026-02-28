using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Events
{
    public class DeductPointEvent
    {
        public Guid Id { get; set; }
        public decimal PointToDeduct { get; set; } = 0;
        public Guid? OrderId { get; set; } = null;
    }
}
