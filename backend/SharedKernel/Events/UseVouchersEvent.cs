using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Events
{
    public class UseVouchersEvent
    {
        public List<Guid> VoucherIds { get; set; } = new List<Guid>();
        public Guid OrderId { get; set; } // dùng để tạo voucher usage
        public Guid UserId { get; set; } // dùng để tạo voucher usage
    }
}
