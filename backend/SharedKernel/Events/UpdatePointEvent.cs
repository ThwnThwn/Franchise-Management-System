using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Events
{
    public class UpdatePointEvent
    {
        public Guid Id { get; set; }
        public decimal Point { get; set; } = 0;
        public Guid? OrderId { get; set; } = null;
        public UpdatePointType ActionType { get; set; }
    }

    public enum UpdatePointType
    {
        Minus,
        Add
    }
}
